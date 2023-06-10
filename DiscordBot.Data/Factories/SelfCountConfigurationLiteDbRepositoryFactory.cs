using DiscordBot.Common.Models.Data.Counting;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factories;

internal class SelfCountConfigurationLiteDbRepositoryFactory : BaseLiteDbRepositoryFactory<ISelfCountConfigurationRepository, SelfCountConfigurationRepository> {
    public SelfCountConfigurationLiteDbRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) :
        base(loggerFactory, liteDbManager) { }

    public override bool RequiresGuildId => true;

    public override ISelfCountConfigurationRepository Create(DiscordGuildId guildId) {
        return new SelfCountConfigurationRepository(GetLogger(), LiteDbManager.GetDatabase(guildId));
    }

    public override ISelfCountConfigurationRepository Create() {
        throw new NotImplementedException();
    }
}
