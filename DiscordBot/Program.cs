using DiscordBot.Configuration;
using DiscordBot.Data.Configuration;
using DiscordBot.Services.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using WiseOldManConnector.Configuration;

namespace DiscordBot;

internal class Program {
    public async Task EntryPointAsync() {
        var config = BuildConfig();
        var services = ConfigureServices(config); // No using statement?
        //var schedulerTask = CreateQuartzScheduler();

        try {
            var bot = new DiscordBot(config, services, services.GetRequiredService<ILogger<DiscordBot>>());
            await bot.Run(new CancellationToken());
            await Task.Delay(-1);
        } catch (Exception e) {
            Log.Fatal(e, "FATAL ERROR: ");
        }
        finally {
            Log.CloseAndFlush();
        }
    }

    private static void Main() {
        new Program().EntryPointAsync().GetAwaiter().GetResult();
    }

    private void ConfigureSerilogger() {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .MinimumLevel.Debug()
            .WriteTo.File(new JsonFormatter(), "logs/osrs_bot.log", rollingInterval: RollingInterval.Day)
            .WriteTo.Console(LogEventLevel.Information)
            .CreateLogger();
    }

    private IServiceProvider ConfigureServices(IConfiguration config) {
        var serviceCollection = new ServiceCollection();

        ConfigureSerilogger();

        serviceCollection
            .AddDiscordBot<Program>(config)
            .UseLiteDbRepositories(config)
            .AddWiseOldManApi()
            .AddDiscordBotServices()
            .ConfigureQuartz(config);

        return serviceCollection.BuildServiceProvider();
    }

    private IConfiguration BuildConfig() {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{environmentName}.json", true).Build();
    }
}
