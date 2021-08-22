using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Common.Configuration;
using DiscordBot.Configuration;
using DiscordBot.Data.Repository;
using DiscordBot.Data.Repository.Migrations;
using DiscordBot.Services;
using DiscordBot.Services.interfaces;
using DiscordBot.Services.Services;
using DiscordBot.Services.Services.interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using WiseOldManConnector.Configuration;
using WiseOldManConnector.Interfaces;

namespace DiscordBot {
    internal class Program {
        public async Task EntryPointAsync() {
            IConfiguration config = BuildConfig();
            IServiceProvider services = ConfigureServices(config); // No using statement?
            //var schedulerTask = CreateQuartzScheduler();

            try {
                var bot = new DiscordBot(config, services);
                await bot.Run(new CancellationToken());
                await Task.Delay(-1);
            } catch (Exception e) {
                Log.Fatal(e, $"FATAL ERROR: ");
            } finally {
                Log.CloseAndFlush();
            }
        }

        private static void Main() => new Program().EntryPointAsync().GetAwaiter().GetResult();
        
        private IServiceProvider ConfigureServices(IConfiguration config) {
            StartupConfiguration configuration = config.GetSection("Startup").Get<StartupConfiguration>();
            BotConfiguration botConfiguration = config.GetSection("Bot").Get<BotConfiguration>();
            // WiseOldManConfiguration manConfiguration = config.GetSection("WiseOldMan").Get<WiseOldManConfiguration>();
            MetricSynonymsConfiguration metricSynonymsConfiguration =
                config.GetSection("MetricSynonyms").Get<MetricSynonymsConfiguration>();

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .MinimumLevel
                .Debug()
                .WriteTo.RollingFile(new JsonFormatter(), "logs/osrs_bot.log")
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                .CreateLogger();

            return new ServiceCollection()
                // Base
                .AddSingleton<MigrationManager>()
                .AddSingleton<DiscordSocketClient>((provider => {
                    var config = new DiscordSocketConfig
                    {
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 100,
                        GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.GuildMessages | GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMembers | GatewayIntents.Guilds,
                        
                    };
                    var client = new DiscordSocketClient(config);
                    return client;
                }))
                .AddSingleton(x=> Log.Logger)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                // Logging
                .AddSingleton<ILogService, SerilogService>()
                .AddLogging(loginBuilder => loginBuilder.AddSerilog(dispose: true))
                // Extra
                .AddSingleton(config)
                .AddSingleton(botConfiguration)
                .AddSingleton(botConfiguration.Messages)
                .AddSingleton(metricSynonymsConfiguration)
                .AddSingleton<InteractiveService>()
                .AddTransient<IDiscordBotRepository>(x => new LiteDbRepository(x.GetService<ILogger>(),configuration.DatabaseFile, x.GetService<MigrationManager>()))
                .AddTransient<IPlayerService, PlayerService>()
                .AddTransient<IGroupService, GroupService>()
                .AddTransient<IOsrsHighscoreService, WiseOldManConnectorService>()
                .AddTransient<ICounterService, CountService>()
                // Add additional services here
                .AddTransient<IWiseOldManLogger, WisOldManLogger>()
                .AddWiseOldManApi()
                .ConfigureQuartz(config)
                .ConfigureAutoMapper()
                .BuildServiceProvider();
        }

        private IConfiguration BuildConfig() {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true).Build();
        }
    }
}