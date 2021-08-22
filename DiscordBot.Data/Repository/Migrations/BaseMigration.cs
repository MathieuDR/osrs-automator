using LiteDB;
using Serilog;

namespace DiscordBot.Repository.Migrations {
    public abstract class BaseMigration : IMigration {
        private readonly ILogger _logger;

        public BaseMigration(ILogger logger) {
            _logger = logger;
        }

        public abstract int Version { get; }

        public void Up(LiteDatabase database) {
            _logger.Information($"[{{db}}] upgrading database to version {Version}");
            database.BeginTrans();
            DoUp(database);
            database.Commit();

            database.UserVersion = Version;
        }

        public void Down(LiteDatabase database) {
            _logger.Information($"[{{db}}] downgrading database to version {Version - 1}");
            database.BeginTrans();
            DoDown(database);

            database.Commit();
            database.UserVersion = Version - 1;
        }

        public abstract void DoUp(LiteDatabase database);

        public abstract void DoDown(LiteDatabase database);
    }
}
