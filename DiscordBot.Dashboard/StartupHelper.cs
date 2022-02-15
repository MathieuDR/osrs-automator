using Dashboard.Configuration;
using Dashboard.Configuration.Options;
using Dashboard.InputFormatters;
using Dashboard.Models.ApiRequests.DiscordEmbed;
using Dashboard.Transformers;
using DiscordBot.Common.Dtos.Runescape;
using DiscordBot.Configuration;
using DiscordBot.Data.Configuration;
using DiscordBot.Services.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using WiseOldManConnector.Configuration;

namespace Dashboard;

public static class StartupHelper {
    private static ApiOptions GetApiOptions(IConfiguration configuration) {
        return configuration.GetSection("WebApp").GetSection("Api").Get<ApiOptions>();
    }

    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration) {
        var apiOptions = GetApiOptions(configuration);
        services.AddRazorPages();
        services.AddServerSideBlazor();
        services.AddMvc(options => { options.InputFormatters.Add(new BypassFormDataInputFormatter()); });
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
            .AddDiscordBot(configuration, typeof(DiscordBot.DiscordBot))
            .UseLiteDbRepositories(configuration)
            .AddWiseOldManApi()
            .AddDiscordBotServices()
            .ConfigureQuartz(configuration);

        services.AddTransient<IMapper<Embed, RunescapeDrop>, EmbedToRunescapeDropMapper>();
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

        app.UseEndpoints(endpoints => {
            endpoints.MapControllers();
            endpoints.MapBlazorHub();
            endpoints.MapFallbackToPage("/_Host");
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
