using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factories {
    public class PlayerLiteDbRepositoryFactory : BaseLiteDbRepositoryFactory<IPlayerRepository, PlayerRepository> {
        public PlayerLiteDbRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) : base(loggerFactory, liteDbManager) { }

        public override IPlayerRepository Create(ulong guildId) {
            return new PlayerRepository(GetLogger(), LiteDbManager.GetDatabase(guildId));
        }
    }
}