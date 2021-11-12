using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Commands.Interactive;
using DiscordBot.Common.Configuration;
using DiscordBot.Helpers.Extensions;
using DiscordBot.Services;
using DiscordBot.Services.Interfaces;
using DiscordBot.Services.Services;
using Fergun.Interactive;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
                                         GatewayIntents.Guilds,
                    };
                    var client = new DiscordSocketClient(config);
                    return client;
                })
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<InteractiveCommandHandlerService>()
                .AddTransient<ICommandRegistrationService,CommandRegistrationService>()
                .AddSingleton<InteractiveService>()
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
        
        private static IServiceCollection AddHelpers(this IServiceCollection serviceCollection) {
            serviceCollection
                .AddTransient<MetricTypeParser>();

            return serviceCollection;
        }

        private static IServiceCollection AddDiscordCommands(this IServiceCollection serviceCollection) {
            return serviceCollection
                .AddSingleton<PingApplicationCommandHandler>()
                .AddSingleton<ManageCommandsApplicationCommandHandler>()
                .AddSingleton<CountApplicationCommandHandler>()
                .AddSingleton<CountConfigurationApplicationCommandHandler>()
                .AddSingleton<ConfigurationApplicationCommandHandler>()
                .AddSingleton<CreateCompetitionCommandHandler>()
                .AddSingleton<ICommandStrategy>(x => new CommandStrategy(new IApplicationCommandHandler[] {
                    x.GetRequiredService<PingApplicationCommandHandler>(),
                    x.GetRequiredService<ManageCommandsApplicationCommandHandler>(),
                    x.GetRequiredService<CountApplicationCommandHandler>(),
                    x.GetRequiredService<CountConfigurationApplicationCommandHandler>(),
                    x.GetRequiredService<ConfigurationApplicationCommandHandler>(),
                    x.GetRequiredService<CreateCompetitionCommandHandler>()
                }));
        }

        private static IServiceCollection AddConfiguration(this IServiceCollection serviceCollection,
            IConfiguration configuration) {
            var botConfiguration = configuration.GetSection("Bot").Get<BotConfiguration>();

            serviceCollection
                .AddOptions<MetricSynonymsConfiguration>()
                .Bind(configuration.GetSection("MetricSynonyms"));

            serviceCollection.AddSingleton(configuration)
                .AddSingleton(botConfiguration)
                .AddSingleton(botConfiguration.Messages);

            return serviceCollection;
        }


        public static IServiceCollection AddDiscordBot(this IServiceCollection serviceCollection,
            IConfiguration configuration) {
            serviceCollection
                .AddLoggingInformation()
                .AddDiscordClient()
                .AddExternalServices()
                .AddConfiguration(configuration)
                .AddHelpers()
                .ConfigureAutoMapper();

            return serviceCollection;
        }
    }
}
