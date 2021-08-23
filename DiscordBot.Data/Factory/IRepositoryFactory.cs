using System;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factory {
    public interface IRepositoryFactory<out T> where T : IRepository {
        public T Create(ulong guildId);
        bool AppliesTo(Type type);
    }

    public interface IRepositoryStrategy {
        IRepository CreateRepository(Type type);
    }

    public abstract class BaseLiteDbRepositoryFactory<TInterface, TConcrete> : IRepositoryFactory<TInterface> where TInterface : IRepository where TConcrete : TInterface{
        public BaseLiteDbRepositoryFactory(LoggerFactory loggerFactory, LiteDbManager liteDbManager) {
            LoggerFactory = loggerFactory;
            LiteDbManager = liteDbManager;
        }

        public LoggerFactory LoggerFactory { get; }
        public LiteDbManager LiteDbManager { get; }

        public abstract TInterface Create(ulong guildId);

        public bool AppliesTo(Type type) {
            return typeof(TInterface).IsAssignableFrom(typeof(Type));
        }

        protected ILogger<TConcrete> GetLogger()  {
            return LoggerFactory.CreateLogger<TConcrete>();
        }
    }

    public class GuildConfigLiteDbRepositoryFactory : BaseLiteDbRepositoryFactory<IGuildConfigRepository, GuildConfigRepository> {
        public GuildConfigLiteDbRepositoryFactory(LoggerFactory loggerFactory, LiteDbManager liteDbManager) : base(loggerFactory, liteDbManager) { }

        public override IGuildConfigRepository Create(ulong guildId) {
            return new GuildConfigRepository(GetLogger(), LiteDbManager.GetDatabase(guildId));
        }
    }
}
