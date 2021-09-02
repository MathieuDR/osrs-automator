using System;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factories {
    public class CommandInfoRepositoryFactory : BaseLiteDbRepositoryFactory<IApplicationCommandInfoRepository, ApplicationApplicationCommandInfoRepository> {
        public CommandInfoRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) : base(loggerFactory, liteDbManager) { }

        public override bool RequiresGuildId => false;

        public override ApplicationApplicationCommandInfoRepository Create(ulong guildId) {
            throw new NotImplementedException();
        }

        public override IRepository Create() {
            return new ApplicationApplicationCommandInfoRepository(GetLogger(), LiteDbManager.GetCommonDatabase());
        }
    }
}
