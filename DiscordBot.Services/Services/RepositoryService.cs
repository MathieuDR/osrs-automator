using System;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Services; 

internal abstract class RepositoryService : BaseService {
    protected RepositoryService(ILogger logger, IRepositoryStrategy repositoryStrategy) : base(logger) {
        RepositoryStrategy = repositoryStrategy;
    }

    protected IRepositoryStrategy RepositoryStrategy { get; }

    [Obsolete("Method1 is deprecated, please use RepositoryStrategy.")]
    protected T GetRepository<T>(ulong? guildId = null) where T : class, IRepository {
        var type = typeof(T);
        Logger.LogDebug("Trying to create repo: {type}", type.Name);

        if (!guildId.HasValue) {
            return RepositoryStrategy.CreateRepository<T>();
        }

        return RepositoryStrategy.CreateRepository<T>(guildId.Value);
    }
}