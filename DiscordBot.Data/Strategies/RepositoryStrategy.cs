using System;
using System.Linq;
using DiscordBot.Data.Factories;
using DiscordBot.Data.Interfaces;

namespace DiscordBot.Data.Strategies; 

public class RepositoryStrategy : IRepositoryStrategy {
    private readonly IRepositoryFactory[] _factories;

    public RepositoryStrategy(IRepositoryFactory[] factories) {
        _factories = factories ?? throw new ArgumentNullException(nameof(factories));
    }

    public IRepository GetOrCreateRepository(Type type, ulong guildId) {
        var factory = _factories
            .FirstOrDefault(f => f.AppliesTo(type, true));

        if (factory == null) {
            throw new InvalidOperationException($"{type} not registered");
        }

        return factory.Create(guildId);
    }

    public IRepository GetOrCreateRepository(Type type) {
        var factory = _factories
            .FirstOrDefault(f => f.AppliesTo(type, false));

        if (factory == null) {
            throw new InvalidOperationException($"{type} not registered");
        }

        return factory.Create();
    }

    public T GetOrCreateRepository<T>(ulong guildId) {
        return (T) GetOrCreateRepository(typeof(T), guildId);
    }

    public T GetOrCreateRepository<T>() {
        return (T) GetOrCreateRepository(typeof(T));
    }
}