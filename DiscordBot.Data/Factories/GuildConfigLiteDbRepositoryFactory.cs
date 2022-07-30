using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factories;

internal class GuildConfigLiteDbRepositoryFactory : BaseLiteDbRepositoryFactory<IGuildConfigRepository, GuildConfigRepository> {
    public GuildConfigLiteDbRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) : base(loggerFactory, liteDbManager) { }

    public override bool RequiresGuildId => true;

    public override IGuildConfigRepository Create(DiscordGuildId guildId) {
        return new GuildConfigRepository(GetLogger(), LiteDbManager.GetDatabase(guildId));
    }

    public override IRepository Create() {
        throw new NotImplementedException();
    }
}
