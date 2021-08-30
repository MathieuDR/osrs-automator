using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Commands.Interactive;
using DiscordBot.Common.Configuration;
using DiscordBot.Services;
using DiscordBot.Services.Interfaces;
using DiscordBot.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Serilog;
using WiseOldManConnector.Interfaces;

namespace DiscordBot.Configuration {
    public static class ConfigurationExtensions {
        private static IServiceCollection AddDiscordClient(this IServiceCollection serviceCollection) {
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
                .AddSingleton<InteractiveCommandHandlerService>()
                .AddDiscordCommands();

            return serviceCollection;
        }

        private static IServiceCollection AddLoggingInformation(this IServiceCollection serviceCollection) {
            serviceCollection.AddSingleton(_ => Log.Logger)
                .AddSingleton<ILogService, SerilogService>()
                .AddLogging(loginBuilder => loginBuilder.AddSerilog(dispose: true))
                .AddTransient<IWiseOldManLogger, WisOldManLogger>();

            return serviceCollection;
        }

        private static IServiceCollection AddExternalServices(this IServiceCollection serviceCollection) {
            serviceCollection
                .AddTransient<IDiscordService, DiscordService>();

            return serviceCollection;
        }

        private static IServiceCollection AddDiscordCommands(this IServiceCollection serviceCollection) {
            serviceCollection.AddTransient<PingApplicationCommand>();
            return serviceCollection;
        }

        private static IServiceCollection AddConfiguration(this IServiceCollection serviceCollection,
            IConfiguration configuration) {
            var botConfiguration = configuration.GetSection("Bot").Get<BotConfiguration>();
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
                .AddLoggingInformation()
                .AddDiscordClient()
                .AddExternalServices()
                .AddConfiguration(configuration)
                .ConfigureAutoMapper();
        
            return serviceCollection;
        }
    }
}
