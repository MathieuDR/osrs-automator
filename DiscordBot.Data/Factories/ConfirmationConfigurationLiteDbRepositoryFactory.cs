using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factories;

internal class ConfirmationConfigurationLiteDbRepositoryFactory : BaseLiteDbRepositoryFactory<IConfirmConfigurationRepository, ConfirmConfigurationRepository> {
    public ConfirmationConfigurationLiteDbRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) :
        base(loggerFactory, liteDbManager) { }

    public override bool RequiresGuildId => true;

    public override IConfirmConfigurationRepository Create(DiscordGuildId guildId) {
        return new ConfirmConfigurationRepository(GetLogger(), LiteDbManager.GetDatabase(guildId));
    }

    public override IConfirmConfigurationRepository Create() {
        throw new NotImplementedException();
    }
}
