using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;
using Quartz;

namespace DiscordBot.Services.Jobs {
    public abstract class BaseJob : IJob {
        public BaseJob(ILogger logger) {
            Logger = logger;
        }

        public ILogger Logger { get; }
        protected IJobExecutionContext Context { get; private set; }
        public IScheduler Scheduler { get; set; }


        public async Task Execute(IJobExecutionContext context) {
            var scopeProps = new Dictionary<string, object> {
                {"jobType", GetType().Name},
                {"jobKey", context.JobDetail.Key},
                {"triggerKey", context.Trigger.Key},
                {"scheduledFor", context.ScheduledFireTimeUtc},
                {"refire", context.RefireCount}
            };

            using (Logger.BeginScope(scopeProps)) {
                Logger.LogInformation("Starting");
                Context = context;
                var result = await DoWork();

                if (result.IsFailed) {
                    Logger.LogError("Failed to do work: {@errors}", result.Errors);
                }
            }
        }

        protected abstract Task<Result> DoWork();
    }
}
