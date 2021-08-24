using System;
using System.Linq;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factory {
    public interface IRepositoryFactory {
        bool AppliesTo(Type type);
        public IRepository Create(ulong guildId);
        // public IRepository Create(ulong guildId);
    }
    public interface IRepositoryFactory<out T> :IRepositoryFactory where T : IRepository {
        
    }

    public interface IRepositoryStrategy {
        IRepository CreateRepository(Type type, ulong guildId);
    }

    public abstract class BaseLiteDbRepositoryFactory<TInterface, TConcrete> : IRepositoryFactory<TInterface> where TInterface : IRepository where TConcrete : TInterface{
        public BaseLiteDbRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) {
            LoggerFactory = loggerFactory;
            LiteDbManager = liteDbManager;
        }

        public ILoggerFactory LoggerFactory { get; }
        public LiteDbManager LiteDbManager { get; }

        public abstract IRepository Create(ulong guildId);


        public bool AppliesTo(Type type) {
            return typeof(TInterface).IsAssignableFrom(type);
        }

        protected ILogger<TConcrete> GetLogger()  {
            return LoggerFactory.CreateLogger<TConcrete>();
        }
    }

    public class GuildConfigLiteDbRepositoryFactory : BaseLiteDbRepositoryFactory<IGuildConfigRepository, GuildConfigRepository> {
        public GuildConfigLiteDbRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) : base(loggerFactory, liteDbManager) { }

        public override IGuildConfigRepository Create(ulong guildId) {
            return new GuildConfigRepository(GetLogger(), LiteDbManager.GetDatabase(guildId));
        }
    }
    
    public class UserCountInfoLiteDbRepositoryFactory : BaseLiteDbRepositoryFactory<IUserCountInfoRepository, UserCountInfoRepository> {
        public UserCountInfoLiteDbRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) : base(loggerFactory, liteDbManager) { }

        public override IUserCountInfoRepository Create(ulong guildId) {
            return new UserCountInfoRepository(GetLogger(), LiteDbManager.GetDatabase(guildId));
        }
    }
    
    public class AutomatedJobStateLiteDbRepositoryFactory : BaseLiteDbRepositoryFactory<IAutomatedJobStateRepository, AutomatedJobStateRepository> {
        public AutomatedJobStateLiteDbRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) : base(loggerFactory, liteDbManager) { }

        public override IAutomatedJobStateRepository Create(ulong guildId) {
            return new AutomatedJobStateRepository(GetLogger(), LiteDbManager.GetDatabase(guildId));
        }
    }
    
    
    public class PlayerLiteDbRepositoryFactory : BaseLiteDbRepositoryFactory<IPlayerRepository, PlayerRepository> {
        public PlayerLiteDbRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) : base(loggerFactory, liteDbManager) { }

        public override IPlayerRepository Create(ulong guildId) {
            return new PlayerRepository(GetLogger(), LiteDbManager.GetDatabase(guildId));
        }
    }

    public class RepositoryStrategy : IRepositoryStrategy {
        private readonly IRepositoryFactory[] _factories;

        public RepositoryStrategy(IRepositoryFactory[] factories) {
            _factories = factories ?? throw new ArgumentNullException(nameof(factories));
        }

        public IRepository CreateRepository(Type type, ulong guildId) {
            var factory = _factories
                .FirstOrDefault(f => f.AppliesTo(type));
            
            if (factory == null) {
                throw new InvalidOperationException($"{type} not registered");
            }

            return factory.Create(guildId);
        }
    }

}
