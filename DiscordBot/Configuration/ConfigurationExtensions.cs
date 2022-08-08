using Discord.Commands;
using DiscordBot.Commands.Interactive;
using DiscordBot.Common.Configuration;
using DiscordBot.Data.Interfaces;
using DiscordBot.Services;
using DiscordBot.Services.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
			.AddSingleton<InteractiveCommandHandlerService>()
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
			.AddTransient<IDiscordService, DiscordService>()
			.AddMediatR(typeof(Program));

		return serviceCollection;
	}

	private static IServiceCollection AddHelpers(this IServiceCollection serviceCollection) {
		serviceCollection
			.AddTransient<MetricTypeParser>();

		return serviceCollection;
	}

	private static IServiceCollection AddDiscordCommands(this IServiceCollection serviceCollection) {
		return serviceCollection
			.AddSingleton<ICommandAuthorizationService, CommandAuthorizationService>()
			.AddSingleton<PingApplicationCommandHandler>()
			.AddSingleton<ManageCommandsApplicationCommandHandler>()
			.AddSingleton<KillBotCommandHandler>()
			.AddSingleton<CountConfigurationApplicationCommandHandler>()
			.AddSingleton<ConfigureApplicationCommandHandler>()
			.AddSingleton<CreateCompetitionCommandHandler>()
			.AddSingleton<AuthorizationConfigurationCommandHandler>()
			.AddSingleton<ICommandStrategy>(x => new CommandStrategy(
				x.GetRequiredService<ILogger<CommandStrategy>>(),
				new IApplicationCommandHandler[] {
					x.GetRequiredService<PingApplicationCommandHandler>(),
					x.GetRequiredService<ManageCommandsApplicationCommandHandler>(),
					x.GetRequiredService<CountConfigurationApplicationCommandHandler>(),
					x.GetRequiredService<KillBotCommandHandler>(),
					x.GetRequiredService<ConfigureApplicationCommandHandler>(),
					x.GetRequiredService<CreateCompetitionCommandHandler>(),
					x.GetRequiredService<AuthorizationConfigurationCommandHandler>()
				}, 
				x.GetRequiredService<ICommandAuthorizationService>()));
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


	private static IServiceCollection AddCommandsFromAssemblies(this IServiceCollection serviceCollection, params Type[] assemblyTypes) {
		// Register provider
		serviceCollection.AddSingleton<ICommandDefinitionProvider>(x => new CommandDefinitionProvider(assemblyTypes, x));

		// Register instigator
		serviceCollection.AddSingleton<ICommandInstigator>(x => new CommandInstigator(
			x.GetRequiredService<IMediator>(),
			x.GetRequiredService<ICommandDefinitionProvider>(), 
			assemblyTypes.GetConcreteClassFromType(typeof(ICommandRequest<>)),
			x.GetRequiredService<ILogger<CommandInstigator>>(),
			x.GetRequiredService<ICommandAuthorizationService>()));

		// Add RegistrationService
		serviceCollection.AddTransient<ICommandRegistrationService, CommandRegistrationService>()
			.Decorate<ICommandRegistrationService>((inner, provider) => new CommandDefinitionRegistrationService(
				provider.GetRequiredService<ILogger<CommandDefinitionRegistrationService>>(),
				provider.GetRequiredService<DiscordSocketClient>(),
				provider.GetRequiredService<IApplicationCommandInfoRepository>(),
				provider.GetRequiredService<ICommandDefinitionProvider>(),
				inner));

		return serviceCollection;
	}

	public static IServiceCollection AddDiscordBot(this IServiceCollection serviceCollection, IConfiguration configuration,
		params Type[] assemblies) {
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


	public static IServiceCollection AddDiscordBot<T>(this IServiceCollection serviceCollection, IConfiguration configuration) =>
		serviceCollection.AddDiscordBot(configuration, typeof(T));

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
