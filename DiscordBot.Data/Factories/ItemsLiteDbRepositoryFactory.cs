using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factories;

internal class ItemsLiteDbRepositoryFactory : BaseLiteDbRepositoryFactory<IItemsRepository, ItemsRepository> {
    public ItemsLiteDbRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) :
        base(loggerFactory, liteDbManager) { }

    public override bool RequiresGuildId => true;

    public override IItemsRepository Create(DiscordGuildId guildId) {
        return new ItemsRepository(GetLogger(), LiteDbManager.GetDatabase(guildId));
    }

    public override IClanFundsRepository Create() {
        throw new NotImplementedException();
    }
}
