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

        public async Task Execute(IJobExecutionContext context) {
            Logger.LogTrace("Starting base job");
            Context = context;

            Logger.LogTrace("Doing work job");
            var result = await DoWork();

            if (result.IsFailed) {
                Logger.LogError("Failed to do work: {@errors}", result.Errors);
            }
        }

        protected abstract Task<Result> DoWork();
    }
}
