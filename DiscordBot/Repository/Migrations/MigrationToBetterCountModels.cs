using LiteDB;
using Serilog;

namespace DiscordBot.Repository.Migrations {
    public sealed class MigrationToBetterCountModels : BaseMigration {
        public MigrationToBetterCountModels(ILogger logger) : base(logger) { }
        public override int Version => 1;

        public override void DoUp(LiteDatabase database) { }

        public override void DoDown(LiteDatabase database) { }
    }
}
