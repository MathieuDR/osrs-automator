    using System.Globalization;
    using DiscordBot.Common.Dtos.Runescape;
using DiscordBot.Configuration;
using DiscordBot.Dashboard.Binders.RouteConstraints;
using DiscordBot.Dashboard.Configuration;
using DiscordBot.Dashboard.Configuration.Options;
using DiscordBot.Dashboard.InputFormatters;
using DiscordBot.Dashboard.Models.ApiRequests.DiscordEmbed;
using DiscordBot.Dashboard.Services;
using DiscordBot.Dashboard.Transformers;
using DiscordBot.Data.Configuration;
using DiscordBot.Services.Configuration;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
    using Serilog;
    using Swashbuckle.AspNetCore.SwaggerGen;
using WiseOldManConnector.Configuration;

namespace DiscordBot.Dashboard;

public static class StartupHelper {
    private static ApiOptions GetApiOptions(IConfiguration configuration) {
        return configuration.GetSection("WebApp").GetSection("Api").Get<ApiOptions>();
    }
    
    private static OAuthOptions GetOAuthOptions(IConfiguration configuration) {
        return configuration.GetSection("WebApp").GetSection("OAuth").Get<OAuthOptions>();
    }

    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration) {
        var apiOptions = GetApiOptions(configuration);
        var oauthOpts = GetOAuthOptions(configuration);
        
        services.AddDiscordOauth(oauthOpts);

        services.AddMvc(options => { options.InputFormatters.Add(new BypassFormDataInputFormatter()); });
        services.Configure<RouteOptions>(options =>
        {
            options.ConstraintMap.Add(UlongRouteConstraint.UlongRouteConstraintName, typeof(UlongRouteConstraint));
        });
        
        services.AddApiVersioning(options => {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(apiOptions.VersionMajor, apiOptions.VersionMinor);
            options.ReportApiVersions = true;
        });

        

        services.AddVersionedApiExplorer(
            options => {
                // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                // note: the specified format code will format the version as "'v'major[.minor][-status]"
                options.GroupNameFormat = "'v'VVV";

                // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                // can also be used to control the format of the API version in route templates
                options.SubstituteApiVersionInUrl = true;
            });

        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerGenOptions>();
        services.AddSwaggerGen(options => {
            // add a custom operation filter which sets default values
            options.OperationFilter<SwaggerDefaultValues>();
            options.SwaggerDoc(apiOptions.Version, new OpenApiInfo { Title = apiOptions.Description, Version = apiOptions.Version });
        });
        
        services
            .AddDiscordBot(configuration, typeof(global::DiscordBot.Bot))
            .UseLiteDbRepositories(configuration)
            .AddWiseOldManApi()
            .AddDiscordBotServices()
            .ConfigureQuartz(configuration);

        services.AddTransient<IMapper<Embed, RunescapeDrop>, EmbedToRunescapeDropMapper>();
        services.AddSingleton<ICachedDiscordService, CachedCachedDiscordService>();
    }


    private static void AddDiscordOauth(this IServiceCollection services, OAuthOptions opts) {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer()
            .AddDiscord(o => {
            o.Prompt = "consent";
            o.Scope.Add("guilds");
            o.Scope.Add("guilds.members.read");
            o.Scope.Add("identify");
            o.ClientId = opts.ClientId;
            o.ClientSecret = opts.ClientSecret;
            o.ReturnUrlParameter = opts.RedirectUrl;
            
            Log.Information("Opts: {@opts}",opts);
            
            o.ClaimActions.MapCustomJson("urn:discord:avatar:url", user =>
                string.Format(
                    CultureInfo.InvariantCulture,
                    "https://cdn.discordapp.com/avatars/{0}/{1}.{2}",
                    user.GetString("id"),
                    user.GetString("avatar"),
                    user.GetString("avatar")!.StartsWith("a_") ? "gif" : "png"));
        });
        
        services.AddAuthorization(options =>
        {
            options.DefaultPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        });
    }

    public static void ConfigurePipeline(IApplicationBuilder app, IWebHostEnvironment env,
        IApiVersionDescriptionProvider apiVersionDescriptionProvider, IConfiguration configuration) {
        if (env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        } else {
            app.UseExceptionHandler("/Error");
        }

        ConfigureSwagger(app, apiVersionDescriptionProvider, GetApiOptions(configuration));
        
        
        app.UseStaticFiles();
        app.UseRouting();
        
        app.UseAuthentication();
        app.UseAuthorization();

    
        app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
        });
    }

    private static void ConfigureSwagger(IApplicationBuilder app, IApiVersionDescriptionProvider provider, ApiOptions options) {
        app.UseSwagger();
        app.UseSwaggerUI(swaggerUiOptions => {
            // build a swagger endpoint for each discovered API version
            foreach (var description in provider.ApiVersionDescriptions) {
                swaggerUiOptions.SwaggerEndpoint($"{description.GroupName}/{options.UIEndpointSuffix}", description.GroupName.ToUpperInvariant());
            }
        });
    }
}
