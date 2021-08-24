using System;
using DiscordBot.Data.Interfaces;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factories {
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
}