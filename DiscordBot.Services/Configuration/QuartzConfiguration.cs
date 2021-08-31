
using System;
using System.Linq;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Services.Jobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace DiscordBot.Services.Configuration {
    public static partial class ServiceConfigurationExtensions {
        public static IServiceCollection ConfigureQuartz(this IServiceCollection services, IConfiguration config) {
            // https://www.quartz-scheduler.net/documentation/quartz-3.x/packages/microsoft-di-integration.html#di-aware-job-factories
            
            // base configuration for DI, read from appSettings.json
            services.Configure<QuartzOptions>(config.GetSection("Quartz"));

            // if you are using persistent job store, you might want to alter some options
            services.Configure<QuartzOptions>(options =>
            {
                options.Scheduling.IgnoreDuplicates = true; // default: false
                options.Scheduling.OverWriteExistingData = true; // default: true
            });
            
            return services
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
                    
                    q.UseDefaultThreadPool(tp =>
                    {
                        tp.MaxConcurrency = 10;
                    });

                    // these are the defaults
                    q.UseSimpleTypeLoader();
                    q.UseInMemoryStore();
                });
            
        }

        private static IServiceCollectionQuartzConfigurator ConfigureJobs(
            this IServiceCollectionQuartzConfigurator quartzServices) {
            var timeZone = TimeZoneInfo.Local;
            if (TimeZoneInfo.GetSystemTimeZones().Any(x => x.Id == "Europe/Berlin")) {
                timeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");
            }

            quartzServices.ScheduleJob<AutoUpdateGroupJob>(trigger =>
                trigger.WithIdentity(JobType.GroupUpdate + "-evening", "wom")
                    .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(22, 00)
                        .InTimeZone(timeZone)
                        .WithMisfireHandlingInstructionFireAndProceed())
                    .WithDescription("Updating in the evening")
            );

            quartzServices.ScheduleJob<AutoUpdateGroupJob>(trigger =>
                trigger.WithIdentity(JobType.GroupUpdate + "-morning", "wom")
                    .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(01, 00)
                        .InTimeZone(timeZone)
                        .WithMisfireHandlingInstructionFireAndProceed())
                    .WithDescription("Updating in the morning")
            );

            quartzServices.ScheduleJob<TopLeaderBoardJob>(trigger =>
                trigger.WithIdentity(JobType.MonthlyTop.ToString(), "wom")
                    .WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(01, 00, 30)
                        .InTimeZone(timeZone)
                        .WithMisfireHandlingInstructionFireAndProceed())
                    .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(10)))
                    .WithDescription("Showing top hiscores for all the servers!")
            );

            quartzServices.ScheduleJob<MonthlyTopDeltasJob>(trigger =>
                trigger.WithIdentity(JobType.MonthlyTopGains.ToString(), "wom")
                    .WithSchedule(CronScheduleBuilder.MonthlyOnDayAndHourAndMinute(01, 00, 5)
                        .InTimeZone(timeZone)
                        .WithMisfireHandlingInstructionFireAndProceed())
                    .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(10)))
                    .WithDescription("Showing top gains for all the servers!")
            );

            return quartzServices;
        }
    }
}
