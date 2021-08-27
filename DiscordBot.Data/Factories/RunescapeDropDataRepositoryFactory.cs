using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Factories {
    public class RunescapeDropDataRepositoryFactory : BaseLiteDbRepositoryFactory<IRuneScapeDropDataRepository, RuneScapeDropDataRepository> {
        public RunescapeDropDataRepositoryFactory(ILoggerFactory loggerFactory, LiteDbManager liteDbManager) : base(loggerFactory, liteDbManager) { }

        public override bool RequiresGuildId => false;

        public override IRuneScapeDropDataRepository Create(ulong guildId) {
            throw new System.NotImplementedException();
        }

        public override IRepository Create() {
            return new RuneScapeDropDataRepository(GetLogger(), LiteDbManager.GetCommonDatabase());
        }
    }
}
