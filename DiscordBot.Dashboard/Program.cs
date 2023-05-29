using Asp.Versioning.ApiExplorer;
using DiscordBot.Dashboard;
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
	builder.Services.AddHostedService<DiscordBot.Bot>();

	var app = builder.Build();
	// StartupHelper.ConfigurePipeline(app, app.Environment, app.Services.GetRequiredService<IApiVersionDescriptionProvider>(), builder.Configuration);

	app.Run();
} catch (Exception e) {
	Log.Fatal(e, "FATAL ERROR: ");
}
finally {
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
