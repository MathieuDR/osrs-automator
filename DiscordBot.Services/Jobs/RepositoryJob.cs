using DiscordBot.Data.Strategies;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Jobs {
    public abstract class RepositoryJob : BaseJob {
        protected RepositoryJob(ILogger logger, IRepositoryStrategy repositoryStrategy) : base(logger) {
            RepositoryStrategy = repositoryStrategy;
        }

        public IRepositoryStrategy RepositoryStrategy { get; }
    }
}
