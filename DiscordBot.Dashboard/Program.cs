using Dashboard;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

// Creates logger
CreateLogger();
try {
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.WebHost.UseSerilog();

    // Add services to the container.
    StartupHelper.ConfigureServices(builder.Services, builder.Configuration);
    builder.WebHost.UseUrls("http://*:5829");
    
    //Add bot as hosted service
    builder.Services.AddHostedService<DiscordBot.DiscordBot>();

    var app = builder.Build();
    StartupHelper.ConfigurePipeline(app, app.Environment, app.Services.GetRequiredService<IApiVersionDescriptionProvider>(), builder.Configuration);
    
    app.Run();
} catch (Exception e) {
    Log.Fatal(e, "FATAL ERROR: ");
} finally {
    Log.CloseAndFlush();
}

static void CreateLogger() {
    Log.Logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .MinimumLevel
        .Debug()
        .WriteTo.File(new JsonFormatter(), "logs/web.log", rollingInterval: RollingInterval.Day)
        .WriteTo.Console(LogEventLevel.Information)
        .CreateLogger();
}

// var builder = WebApplication.CreateBuilder(args);
//
// // Add services to the container.
// builder.Services.AddRazorPages();
// builder.Services.AddServerSideBlazor();
// builder.Services.AddSingleton<WeatherForecastService>();
//
// var app = builder.Build();
//
// // Configure the HTTP request pipeline.
// if (!app.Environment.IsDevelopment()) {
//     app.UseExceptionHandler("/Error");
//     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//     app.UseHsts();
// }
//
// app.UseHttpsRedirection();
//
// app.UseStaticFiles();
//
// app.UseRouting();
//
// app.MapBlazorHub();
// app.MapFallbackToPage("/_Host");
//
// app.Run();
