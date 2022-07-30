using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factories;

internal class RunescapeDropDataRepositoryFactory : BaseLiteDbRepositoryFactory<IRuneScapeDropDataRepository, RuneScapeDropDataRepository> {
    public RunescapeDropDataRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) : base(loggerFactory, liteDbManager) { }

    public override bool RequiresGuildId => false;

    public override IRuneScapeDropDataRepository Create(DiscordGuildId guildId) {
        throw new NotImplementedException();
    }

    public override IRuneScapeDropDataRepository Create() {
        return new RuneScapeDropDataRepository(GetLogger(), LiteDbManager.GetCommonDatabase());
    }
}