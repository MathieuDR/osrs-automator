using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Services {
    public abstract class BaseService {
        public IRepositoryStrategy RepositoryStrategy { get; }
        private readonly ILogger _logger;

        public BaseService(ILogger logger, IRepositoryStrategy repositoryStrategy) {
            RepositoryStrategy = repositoryStrategy;
            _logger = logger;
        }

        protected T GetRepository<T>(ulong guildId) where T : class, IRepository{
            var type = typeof(T);
            return RepositoryStrategy.CreateRepository(type, guildId) as T;
        }
    }
}