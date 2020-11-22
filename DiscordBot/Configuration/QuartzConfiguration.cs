using System;
using DiscordBotFanatic.Jobs;
using DiscordBotFanatic.Models.Enums;
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

                    q.ConfigureJobs();
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

        private static IServiceCollectionQuartzConfigurator ConfigureJobs(
            this IServiceCollectionQuartzConfigurator quartzServices) {
            quartzServices.ScheduleJob<AutoUpdateGroupJob>(trigger => 
                trigger.WithIdentity(JobType.GroupUpdate.ToString(), "wom")
                    .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(5)))
                    .WithSimpleSchedule(x=>x.WithIntervalInHours(2).RepeatForever())
                    .WithDescription("Showing achievements for all the servers!")
            );

            quartzServices.ScheduleJob<AchievementsJob>(trigger => 
                trigger.WithIdentity(JobType.Achievements.ToString(), "wom")
                .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddMinutes(1)))
                .WithSimpleSchedule(x=>x.WithIntervalInHours(1).RepeatForever())
                .WithDescription("Showing achievements for all the servers!")
            );

            return quartzServices;
        }
    }
}