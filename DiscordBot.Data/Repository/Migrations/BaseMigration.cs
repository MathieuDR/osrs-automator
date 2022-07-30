using LiteDB;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Data.Repository.Migrations;

public abstract class BaseMigration : IMigration {
    private readonly ILogger _logger;

    public BaseMigration(ILogger logger) {
        _logger = logger;
    }

    public abstract int Version { get; }

    public void Up(LiteDatabase database) {
        _logger.LogInformation("[{{db}}] upgrading database to version {version}", Version);
        database.BeginTrans();
        DoUp(database);
        database.Commit();

        database.UserVersion = Version;
    }

    public void Down(LiteDatabase database) {
        _logger.LogInformation("[{{db}}] downgrading database to version {version}",Version - 1);
        database.BeginTrans();
        DoDown(database);

        database.Commit();
        database.UserVersion = Version - 1;
    }

    public abstract void DoUp(LiteDatabase database);

    public abstract void DoDown(LiteDatabase database);
}
