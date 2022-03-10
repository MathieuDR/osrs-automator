using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factories;

internal class CommandInfoRepositoryFactory : BaseLiteDbRepositoryFactory<IApplicationCommandInfoRepository, ApplicationCommandInfoRepository> {
    public CommandInfoRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) : base(loggerFactory, liteDbManager) { }

    public override bool RequiresGuildId => false;

    public override IApplicationCommandInfoRepository Create(DiscordGuildId guildId) {
        throw new NotImplementedException();
    }

    public override IApplicationCommandInfoRepository Create() {
        return new ApplicationCommandInfoRepository(GetLogger(), LiteDbManager.GetCommonDatabase());
    }
}
