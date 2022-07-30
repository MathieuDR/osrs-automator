using DiscordBot.Data.Interfaces;

namespace DiscordBot.Data.Strategies;

public interface IRepositoryStrategy {
    IRepository GetOrCreateRepository(Type type, DiscordGuildId guildId);
    IRepository GetOrCreateRepository(Type type);

    T GetOrCreateRepository<T>(DiscordGuildId guildId);
    T GetOrCreateRepository<T>();
}
