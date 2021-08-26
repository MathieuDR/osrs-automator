using System.Threading.Tasks;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Jobs {
    public class HandleRunescapeDropJob : BaseJob {
        public HandleRunescapeDropJob(ILogger<HandleRunescapeDropJob> logger) : base(logger) { }
        protected override Task<Result> DoWork() {
            Logger.LogInformation("Now we would handle our stuff");

            return Task.FromResult(Result.Ok());
        }
    }
}
