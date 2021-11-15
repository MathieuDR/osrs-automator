using System;
using DiscordBot.Data.Interfaces;

namespace DiscordBot.Data.Strategies; 

public interface IRepositoryStrategy {
    IRepository GetOrCreateRepository(Type type, ulong guildId);
    IRepository GetOrCreateRepository(Type type);

    T GetOrCreateRepository<T>(ulong guildId);
    T GetOrCreateRepository<T>();
}