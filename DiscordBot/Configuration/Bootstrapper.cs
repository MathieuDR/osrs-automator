using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Common.Configuration;
using DiscordBot.Data;
using DiscordBot.Data.Configuration;
using DiscordBot.Data.Factories;
using DiscordBot.Data.Repository.Migrations;
using DiscordBot.Data.Strategies;
using DiscordBot.Services;
using DiscordBot.Services.Configuration;
using DiscordBot.Services.interfaces;
using DiscordBot.Services.Interfaces;
using DiscordBot.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WiseOldManConnector.Configuration;
using WiseOldManConnector.Interfaces;

namespace DiscordBot.Configuration {
    public static class Bootstrapper {
        private static IServiceCollection AddDataConnection(this IServiceCollection serviceCollection,
            IConfiguration configuration) {

            serviceCollection
                .AddSingleton<MigrationManager>()
                .AddSingleton<LiteDbManager>()
                .AddTransient<GuildConfigLiteDbRepositoryFactory>()
                .AddTransient<PlayerLiteDbRepositoryFactory>()
                .AddTransient<UserCountInfoLiteDbRepositoryFactory>()
                .AddTransient<AutomatedJobStateLiteDbRepositoryFactory>()
                .AddSingleton<IRepositoryStrategy>(x =>
                    new RepositoryStrategy(new IRepositoryFactory[] {
                        x.GetRequiredService<PlayerLiteDbRepositoryFactory>(),
                        x.GetRequiredService<GuildConfigLiteDbRepositoryFactory>(),
                        x.GetRequiredService<UserCountInfoLiteDbRepositoryFactory>(),
                        x.GetRequiredService<AutomatedJobStateLiteDbRepositoryFactory>()
                    }))
                .AddOptions<LiteDbOptions>().Configure<IConfiguration>(((options, configuration1) => configuration.GetSection(LiteDbOptions.SectionName).Bind(options)));
                // .AddTransient<IDiscordBotRepository>(x => new LiteDbRepository(x.GetService<ILogger>(),
                //     config.DatabaseFile, x.GetService<MigrationManager>()));


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
                .AddTransient<IDiscordService, DiscordService>();

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
                .AddDiscordBotServices()
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
