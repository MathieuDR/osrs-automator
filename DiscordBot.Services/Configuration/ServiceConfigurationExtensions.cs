using DiscordBot.Common.Configuration;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.ExternalServices;
using DiscordBot.Services.Helpers;
using DiscordBot.Services.HttpClients;
using DiscordBot.Services.Interfaces;
using DiscordBot.Services.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;

namespace DiscordBot.Services.Configuration;

public static partial class ServiceConfigurationExtensions {
    public static IServiceCollection AddDiscordBotServices(this IServiceCollection serviceCollection) =>
        serviceCollection
            .AddServices()
            .AddExternalServices();

    private static IServiceCollection AddExternalServices(this IServiceCollection serviceCollection) {
        serviceCollection
            .AddRefitClient<IOsrsWikiApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://oldschool.runescape.wiki/"))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpLoggingHandler());

        serviceCollection.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(typeof(ServiceConfigurationExtensions).Assembly); });

        return serviceCollection;
    }


    private static IServiceCollection AddServices(this IServiceCollection serviceCollection) {
        serviceCollection
            .AddSingleton<ICollectionLogItemProvider, CollectionLogItemProvider>()
            .AddTransient<IPlayerService, PlayerService>()
            .AddTransient<IGroupService, GroupService>()
            .AddTransient<IOsrsHighscoreService, WiseOldManConnectorService>()
            .AddTransient<ICounterService, CountService>()
            .AddTransient<IAutomatedDropperService, AutomatedDropperService>()
            .AddTransient<IAuthorizationService, AuthorizationService>()
            .AddTransient<IGraveyardService, GraveyardService>()
            .AddTransient<IClanFundsService, ClanFundsService>()
            .AddTransient<IConfirmationService, ConfirmationService>()
            .AddSingleton<IClock, SystemClock>()
            .AddSingleton<IHostingService>(sp => new HostingService(
                sp.GetRequiredService<ILogger<HostingService>>(),
                sp.GetRequiredService<IRepositoryStrategy>(),
                sp.GetRequiredService<MessageConfiguration>(),
                sp.GetRequiredService<IOptions<BotTeamConfiguration>>(),
                sp.GetRequiredService<IClock>()));

        return serviceCollection;
    }
}
