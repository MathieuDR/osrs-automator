using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Configuration;
using DiscordBot.Models.Configuration;
using DiscordBot.Repository;
using DiscordBot.Services;
using DiscordBot.Services.interfaces;
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
                var discordTask = ConfigureDiscord(services, config);
                var schedulerTask = ConfigureScheduler(services);

                //var scheduler = await schedulerTask;
                //await scheduler.Start();
                //await ScheduleTasks(scheduler);

                await discordTask;
                await schedulerTask;

                await Task.Delay(-1);
            } catch (Exception e) {
                Log.Fatal(e, $"FATAL ERROR: ");
                //await (await schedulerTask).Shutdown();
            }
        }

        private static void Main() => new Program().EntryPointAsync().GetAwaiter().GetResult();

        private async Task ConfigureScheduler(IServiceProvider services) {
            var factory = services.GetRequiredService<ISchedulerFactory>();
            IScheduler scheduler = await factory.GetScheduler();
            await scheduler.Start();
        }

        private async Task ConfigureDiscord(IServiceProvider services, IConfiguration config) {
            DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
            await ((CommandHandlingService) services.GetRequiredService(typeof(CommandHandlingService)))
                .InitializeAsync(services);

            var botConfig = config.GetSection("Bot").Get<BotConfiguration>();
            
            await client.LoginAsync(TokenType.Bot, botConfig.Token);
            await client.StartAsync();
        }
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
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                // Logging
                // ReSharper disable once ObjectCreationAsStatement
                //.AddLogging(builder => builder.AddConsole(x=> new ConsoleLoggerOptions(){LogToStandardErrorThreshold = LogLevel.Information}))
                .AddSingleton<ILogService, SerilogService>()
                .AddLogging(loginBuilder => loginBuilder.AddSerilog(dispose: true))
                // Extra
                .AddSingleton(config)
                .AddSingleton(botConfiguration)
                .AddSingleton(botConfiguration.Messages)
                .AddSingleton(metricSynonymsConfiguration)
                .AddSingleton<InteractiveService>()
                //.AddTransient<HighscoreService>()
                .AddTransient<IDiscordBotRepository>(x => new LiteDbRepository(configuration.DatabaseFile))
                .AddTransient<IPlayerService, PlayerService>()
                .AddTransient<IGroupService, GroupService>()
                .AddTransient<ICompetitionService, CompetitionService>()
                .AddTransient<IAuthenticationService, AuthenticationService>()
                .AddTransient<IOsrsHighscoreService, WiseOldManConnectorService>()
                .AddTransient<ICounterService, CountService>()
                //.AddTransient<IOsrsHighscoreService, HighscoreService>()
                //Omage services
                //.AddTransient<IImageService<MetricInfo>, MetricImageService>()
                //.AddTransient<IImageService<DeltaInfo>, DeltaImageService>()
                //.AddTransient<IImageService<RecordInfo>, RecordImageService>()
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