using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Api;
using WiseOldManConnector.Interfaces;

namespace WiseOldManConnector.Configuration {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection AddWiseOldManApi(this IServiceCollection services) {
            services.AddTransient<IWiseOldManPlayerApi, PlayerConnector>();
            services.AddTransient<IWiseOldManRecordApi, RecordConnector>();
            services.AddTransient<IWiseOldManGroupApi, GroupConnector>();
            services.AddTransient<IWiseOldManCompetitionApi, CompetitionConnector>();
            return services;
        }
    }
}