using Discord.Commands;
using DiscordBot.Commands.Interactive;
using DiscordBot.Commands.Interactive2.Base.Definitions;
using DiscordBot.Common.Configuration;
using DiscordBot.Services;
using DiscordBot.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Serilog;
using WiseOldManConnector.Interfaces;

namespace DiscordBot.Configuration;

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
            .AddTransient<ICommandRegistrationService, CommandRegistrationService>()
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
            .AddSingleton<ConfigureApplicationCommandHandler>()
            .AddSingleton<CreateCompetitionCommandHandler>()
            .AddSingleton<AuthorizationConfigurationCommandHandler>()
            .AddSingleton<ICommandStrategy>(x => new CommandStrategy(
                x.GetRequiredService<ILogger<CommandStrategy>>(),
                new IApplicationCommandHandler[] {
                    x.GetRequiredService<PingApplicationCommandHandler>(),
                    x.GetRequiredService<ManageCommandsApplicationCommandHandler>(),
                    x.GetRequiredService<CountApplicationCommandHandler>(),
                    x.GetRequiredService<CountConfigurationApplicationCommandHandler>(),
                    x.GetRequiredService<ConfigureApplicationCommandHandler>(),
                    x.GetRequiredService<CreateCompetitionCommandHandler>(),
                    x.GetRequiredService<AuthorizationConfigurationCommandHandler>()
                }, x.GetRequiredService<IGroupService>(), x.GetRequiredService<IOptions<BotTeamConfiguration>>()));
    }

    private static IServiceCollection AddConfiguration(this IServiceCollection serviceCollection,
        IConfiguration configuration) {
        var botConfiguration = configuration.GetSection("Bot").Get<BotConfiguration>();

        serviceCollection
            .AddOptions<MetricSynonymsConfiguration>()
            .Bind(configuration.GetSection("MetricSynonyms"));

        serviceCollection
            .AddOptions<BotConfiguration>()
            .Bind(configuration.GetSection("Bot"));

        serviceCollection
            .AddOptions<BotTeamConfiguration>()
            .Bind(configuration.GetSection("Bot").GetSection(nameof(BotConfiguration.TeamConfiguration)));

        serviceCollection.AddSingleton(configuration)
            .AddSingleton(botConfiguration)
            .AddSingleton(botConfiguration.Messages);

        return serviceCollection;
    }


    
    private static IServiceCollection AddCommandsFromAssemblies(this IServiceCollection serviceCollection, params Type[] assemblTypes) {
        // Get assemblies from types
        var assemblies = assemblTypes.Select(x => x.Assembly).Distinct().ToArray();
        
        // Get all commands from assemblies
        var commands = assemblies.SelectMany(x => x.GetTypes())
            .Where(x => typeof(ICommandDefinition).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface)
            .ToArray();
        
        // Split them up into root and sub commands through interface
        var rootCommands = commands.Where(x => typeof(IRootCommandDefinition).IsAssignableFrom(x)).ToArray();
        var subCommands = commands.Where(x => typeof(ISubCommandDefinition).IsAssignableFrom(x)).Where(x => x.GetInterfaces().Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == typeof(ISubCommandDefinition<>))).ToArray();

        // Place commands in a dictionary based on generic type
        var commandDictionary = new Dictionary<Type, IEnumerable<Type>>();
        foreach (var rootCommand in rootCommands) {
           // Get all subcommands that have a generic parameter of rootcommand
            var subCommandsOfRootCommandOfType = subCommands.Where(x => x.GetInterfaces().Any(y => y.GetGenericArguments().Any(z => z == rootCommand))).ToArray();
            commandDictionary.Add(rootCommand, subCommandsOfRootCommandOfType);
        }
        

        return serviceCollection;
    }
    public static IServiceCollection AddDiscordBot(this IServiceCollection serviceCollection, IConfiguration configuration, params Type[] assemblies) {
        serviceCollection
            .AddLoggingInformation()
            .AddDiscordClient()
            .AddExternalServices()
            .AddConfiguration(configuration)
            .AddHelpers()
            .ConfigureAutoMapper()
            .AddCommandsFromAssemblies(assemblies);

   

        return serviceCollection;
    }


    public static IServiceCollection AddDiscordBot<T>(this IServiceCollection serviceCollection, IConfiguration configuration) {
        return serviceCollection.AddDiscordBot(configuration, typeof(T));
    }

    [Obsolete]
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
