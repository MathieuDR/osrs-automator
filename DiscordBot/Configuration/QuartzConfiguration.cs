using System;
using DiscordBotFanatic.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace DiscordBotFanatic.Configuration {
    public static class QuartzConfiguration {
        public static IServiceCollection ConfigureQuartz(this IServiceCollection services, IConfiguration config) {
            // https://www.quartz-scheduler.net/documentation/quartz-3.x/packages/microsoft-di-integration.html#di-aware-job-factories

            return services
                .Configure<QuartzOptions>(config.GetSection("Quartz"))
                .AddQuartz(q => {
                    // handy when part of cluster or you want to otherwise identify multiple schedulers
                    q.SchedulerId = "Scheduler-Core";

                    q.ConfigureHelloJob();
                    // we take this from appsettings.json, just show it's possible
                    // q.SchedulerName = "Quartz ASP.NET Core Sample Scheduler";

                    // we could leave DI configuration intact and then jobs need
                    // to have public no-arg constructor
                    // the MS DI is expected to produce transient job instances
                    // this WONT'T work with scoped services like EF Core's DbContext
                    q.UseMicrosoftDependencyInjectionJobFactory(options => {
                        // if we don't have the job in DI, allow fallback 
                        // to configure via default constructor
                        options.AllowDefaultConstructor = true;
                    });
                    
                    // or for scoped service support like EF Core DbContext
                    // q.UseMicrosoftDependencyInjectionScopedJobFactory();

                    // these are the defaults
                    q.UseSimpleTypeLoader();
                    q.UseInMemoryStore();
                });
        }

        private static IServiceCollectionQuartzConfigurator ConfigureHelloJob(
            this IServiceCollectionQuartzConfigurator quartzServices) {
            //quartzServices.ScheduleJob<HelloJob>(trigger => trigger
            //    .WithIdentity("Combined configuration Trigger")
            //    .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(20)))
            //    .WithDailyTimeIntervalSchedule(x => x.WithInterval(10, IntervalUnit.Second))
            //    .WithDescription("my awesome trigger configured for a job with single call")
            //);

            quartzServices.ScheduleJob<AchievementsJob>(trigger => 
                trigger.WithIdentity("Combined configuration Trigger")
                .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(30)))
                .WithSimpleSchedule(x=>x.WithIntervalInHours(6).RepeatForever())
                .WithDescription("Showing achievements for all the servers!")
            );

            return quartzServices;
        }
    }
}