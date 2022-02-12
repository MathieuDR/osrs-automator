using Common.Semaphores;
using DiscordBot.Services.ExternalServices;
using DiscordBot.Services.Interfaces;
using DiscordBot.Services.Services;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace DiscordBot.Services.Configuration;

public static partial class ServiceConfigurationExtensions {
    public static IServiceCollection AddDiscordBotServices(this IServiceCollection serviceCollection) {
        return serviceCollection
            .AddServices()
            .AddExternalServices();
    }

    private static IServiceCollection AddExternalServices(this IServiceCollection serviceCollection) {
        serviceCollection
            .AddRefitClient<IOsrsWikiApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://oldschool.runescape.wiki/"));

        return serviceCollection;
    }

    private static IServiceCollection AddServices(this IServiceCollection serviceCollection) {
        serviceCollection
            .AddTransient<ICollectionLogItemProvider, CollectionLogItemProvider>()
            .AddTransient<IPlayerService, PlayerService>()
            .AddTransient<IGroupService, GroupService>()
            .AddTransient<IOsrsHighscoreService, WiseOldManConnectorService>()
            .AddTransient<ICounterService, CountService>()
            .AddTransient<IAutomatedDropperService, AutomatedDropperService>()
            .AddTransient<IAuthorizationService, AuthorizationService>()
            .AddTransient<IGraveyardService, GraveyardService>()
            .AddTransient<IClanFundsService, ClanFundsService>()
            .AddSingleton(new TimeSpanSemaphore(150, TimeSpan.FromSeconds(5*60+10))); // rate limit to 150 requests per 5 minutes + 10 seconds buffer

        return serviceCollection;
    }
}
