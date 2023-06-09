using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factories;

internal class ConfirmationLiteDbRepositoryFactory : BaseLiteDbRepositoryFactory<IConfirmationRepository, ConfirmationRepository> {
    public ConfirmationLiteDbRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) :
        base(loggerFactory, liteDbManager) { }

    public override bool RequiresGuildId => true;

    public override IConfirmationRepository Create(DiscordGuildId guildId) {
        return new ConfirmationRepository(GetLogger(), LiteDbManager.GetDatabase(guildId));
    }

    public override IConfirmationRepository Create() {
        throw new NotImplementedException();
    }
}
