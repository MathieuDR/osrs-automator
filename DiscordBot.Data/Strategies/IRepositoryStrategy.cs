using System;
using DiscordBot.Data.Interfaces;

namespace DiscordBot.Data.Strategies; 

public interface IRepositoryStrategy {
    IRepository CreateRepository(Type type, ulong guildId);
    IRepository CreateRepository(Type type);

    T CreateRepository<T>(ulong guildId);
    T CreateRepository<T>();
}