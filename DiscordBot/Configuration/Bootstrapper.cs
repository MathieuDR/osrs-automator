using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Models.Configuration;
using DiscordBot.Repository;
using DiscordBot.Repository.Migrations;
using DiscordBot.Services;
using DiscordBot.Services.interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WiseOldManConnector.Configuration;
using WiseOldManConnector.Interfaces;

namespace DiscordBot.Configuration {
    public static class Bootstrapper {
        private static IServiceCollection AddDataConnection(this IServiceCollection serviceCollection,
            IConfiguration configuration) {
            var config = configuration.GetSection("Startup").Get<StartupConfiguration>();
            serviceCollection
                .AddSingleton<MigrationManager>()
                .AddTransient<IDiscordBotRepository>(x => new LiteDbRepository(x.GetService<ILogger>(),
                    config.DatabaseFile, x.GetService<MigrationManager>()));


            return serviceCollection;
        }

        private static IServiceCollection AddDiscord(this IServiceCollection serviceCollection) {
            serviceCollection
                .AddSingleton(_ => {
                    var config = new DiscordSocketConfig {
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 100,
                        GatewayIntents = GatewayIntents.GuildMembers | GatewayIntents.GuildMessages |
                                         GatewayIntents.GuildMessageReactions | GatewayIntents.GuildMembers |
                                         GatewayIntents.Guilds
                    };
                    var client = new DiscordSocketClient(config);
                    return client;
                })
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<InteractiveService>();

            return serviceCollection;
        }

        private static IServiceCollection AddLogging(this IServiceCollection serviceCollection) {
            serviceCollection.AddSingleton(_ => Log.Logger)
                .AddSingleton<ILogService, SerilogService>()
                .AddLogging(loginBuilder => loginBuilder.AddSerilog(dispose: true))
                .AddTransient<IWiseOldManLogger, WisOldManLogger>();

            return serviceCollection;
        }

        private static IServiceCollection AddBotServices(this IServiceCollection serviceCollection) {
            serviceCollection
                .AddTransient<IPlayerService, PlayerService>()
                .AddTransient<IGroupService, GroupService>()
                .AddTransient<IOsrsHighscoreService, WiseOldManConnectorService>()
                .AddTransient<ICounterService, CountService>();

            return serviceCollection;
        }

        private static IServiceCollection AddConfiguration(this IServiceCollection serviceCollection,
            IConfiguration configuration) {
            var botConfiguration = configuration.GetSection("Bot").Get<BotConfiguration>();
            // WiseOldManConfiguration manConfiguration = config.GetSection("WiseOldMan").Get<WiseOldManConfiguration>();
            var metricSynonymsConfiguration =
                configuration.GetSection("MetricSynonyms").Get<MetricSynonymsConfiguration>();

            serviceCollection
                .AddSingleton(configuration)
                .AddSingleton(botConfiguration)
                .AddSingleton(botConfiguration.Messages)
                .AddSingleton(metricSynonymsConfiguration);

            return serviceCollection;
        }


        public static IServiceCollection AddDiscordBot(this IServiceCollection serviceCollection,
            IConfiguration configuration) {
            serviceCollection
                .AddLogging()
                .AddDataConnection(configuration)
                .AddDiscord()
                .AddBotServices()
                .AddConfiguration(configuration)
                .ConfigureQuartz(configuration)
                .AddWiseOldManApi()
                .ConfigureAutoMapper();

            return serviceCollection;
        }
    }
}
