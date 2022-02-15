using Common.Semaphores;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Api;
using WiseOldManConnector.Interfaces;

namespace WiseOldManConnector.Configuration;

public static class ServiceCollectionExtensions {
    public static IServiceCollection AddWiseOldManApi(this IServiceCollection services) {
        services.AddTransient<IWiseOldManPlayerApi, PlayerConnector>();
        services.AddTransient<IWiseOldManRecordApi, RecordConnector>();
        services.AddTransient<IWiseOldManGroupApi, GroupConnector>();
        services.AddTransient<IWiseOldManCompetitionApi, CompetitionConnector>();
        services.AddTransient<IWiseOldManNameApi, NameConnector>();
        services.AddSingleton(new TimeSpanSemaphore(150, TimeSpan.FromSeconds(5*60+10))); // rate limit to 150 requests per 5 minutes + 10 seconds buffer
        return services;
    }
}
