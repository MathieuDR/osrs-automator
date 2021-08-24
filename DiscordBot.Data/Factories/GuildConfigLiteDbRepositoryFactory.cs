using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factories {
    public class GuildConfigLiteDbRepositoryFactory : BaseLiteDbRepositoryFactory<IGuildConfigRepository, GuildConfigRepository> {
        public GuildConfigLiteDbRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) : base(loggerFactory, liteDbManager) { }

        public override IGuildConfigRepository Create(ulong guildId) {
            return new GuildConfigRepository(GetLogger(), LiteDbManager.GetDatabase(guildId));
        }
    }
}