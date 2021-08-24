using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factories {
    public class UserCountInfoLiteDbRepositoryFactory : BaseLiteDbRepositoryFactory<IUserCountInfoRepository, UserCountInfoRepository> {
        public UserCountInfoLiteDbRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) : base(loggerFactory, liteDbManager) { }

        public override IUserCountInfoRepository Create(ulong guildId) {
            return new UserCountInfoRepository(GetLogger(), LiteDbManager.GetDatabase(guildId));
        }
    }
}