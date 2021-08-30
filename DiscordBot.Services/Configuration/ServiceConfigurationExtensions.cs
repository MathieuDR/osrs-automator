using System;
using DiscordBot.Services.ExternalServices;
using DiscordBot.Services.Interfaces;
using DiscordBot.Services.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;

namespace DiscordBot.Services.Configuration {
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
                .AddTransient<IAutomatedDropperService, AutomatedDropperService>();

            return serviceCollection;
        }
    }
}
