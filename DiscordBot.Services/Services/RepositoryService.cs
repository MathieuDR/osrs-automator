using System;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Services {
    internal abstract class RepositoryService : BaseService {
        protected RepositoryService(ILogger logger, IRepositoryStrategy repositoryStrategy) : base(logger) {
            RepositoryStrategy = repositoryStrategy;
        }

        public ulong? GuildId { get; set; }

        protected IRepositoryStrategy RepositoryStrategy { get; }

        protected T GetRepository<T>(ulong guildId) where T : class, IRepository {
            var type = typeof(T);
            
            Logger.LogDebug("Trying to create repo: {type}", type.Name);
            return RepositoryStrategy.CreateRepository(type, guildId) as T;
        }
        
        protected T GetRepository<T>() where T : class, IRepository {
            if (!GuildId.HasValue) {
                throw new NullReferenceException(nameof(GuildId));
            }

            return GetRepository<T>(GuildId.Value);
        }
    }
}
