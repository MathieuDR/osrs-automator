using System.Threading.Tasks;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Jobs {
    public class HandleRunescapeDropJob : RepositoryJob {
        private readonly IDiscordService _discordService;
        public HandleRunescapeDropJob(ILogger<HandleRunescapeDropJob> logger, IRepositoryStrategy repositoryStrategy, IDiscordService discordService) : base(logger, repositoryStrategy) {
            _discordService = discordService;
        }
        protected override Task<Result> DoWork() {
            var endpoint = Context.MergedJobDataMap.GetGuidValue("endpoint");
            var repo = RepositoryStrategy.CreateRepository<IRuneScapeDropDataRepository>();
            var data = repo.GetActive(endpoint).Value;

            _discordService.PrintRunescapeDataDrop(data, 403539795944538122, 570935856727326720);
            
            repo.CloseActive(endpoint);
            return Task.FromResult(Result.Ok());
        }
    }
}
