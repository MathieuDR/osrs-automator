using System;
using DiscordBot.Data.Interfaces;

namespace DiscordBot.Data.Factories {
    public interface IRepositoryFactory {
        bool AppliesTo(Type type, bool requiresGuildId);
        public IRepository Create(ulong guildId);
        public IRepository Create();
    }

    public interface IRepositoryFactory<out T> : IRepositoryFactory where T : IRepository { }
}
