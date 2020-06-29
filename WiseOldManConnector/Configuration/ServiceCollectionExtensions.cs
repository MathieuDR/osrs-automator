using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Api;
using WiseOldManConnector.Interfaces;

namespace WiseOldManConnector.Configuration {
    public static class ServiceCollectionExtensions {
        public static void AddWiseOldManApi(this IServiceCollection services) {
            services.AddTransient<IWiseOldManPlayerApi, PlayerConncetor>();
        }
    }
}