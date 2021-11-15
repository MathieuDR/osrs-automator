using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Services;

internal abstract class RepositoryService : BaseService {
    protected RepositoryService(ILogger logger, IRepositoryStrategy repositoryStrategy) : base(logger) {
        RepositoryStrategy = repositoryStrategy;
    }

    protected IRepositoryStrategy RepositoryStrategy { get; }

    /// <summary>
    ///     Gets a repository of the type T
    ///     If no Id is given, the default repository is returned
    /// </summary>
    /// <param name="guildId">Id of a guild</param>
    /// <typeparam name="T">Type of repository</typeparam>
    /// <returns>Repository of Type T</returns>
    protected T GetRepository<T>(ulong? guildId = null) where T : class, IRepository {
        Logger.LogDebug("Trying to create repo: {type}", typeof(T).Name);
        return !guildId.HasValue ? RepositoryStrategy.GetOrCreateRepository<T>() : RepositoryStrategy.GetOrCreateRepository<T>(guildId.Value);
    }
}
