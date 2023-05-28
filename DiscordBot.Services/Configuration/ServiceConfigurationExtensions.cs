using DiscordBot.Services.ExternalServices;
using DiscordBot.Services.HttpClients;
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
        
        // var httpClient = new HttpClient(new HttpLoggingHandler()){ BaseAddress = new Uri("https://oldschool.runescape.wiki/")};
        
        // add refit client "IOsrsWikiApi"
        // with an httpclient that uses HttpLoggingHandler as a delegating handler

        serviceCollection
            .AddRefitClient<IOsrsWikiApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://oldschool.runescape.wiki/"))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpLoggingHandler());


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
            .AddTransient<IClanFundsService, ClanFundsService>();

        return serviceCollection;
    }
}
