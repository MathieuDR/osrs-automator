using System.Collections.Generic;
using DiscordBot.Data.Configuration;
using DiscordBot.Data.Repository.Migrations;
using LiteDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace DiscordBot.Data; 

public class LiteDbManager {
    private readonly object _commonLock = new();
    private readonly object _createLock = new();
    private readonly Dictionary<ulong, LiteDatabase> _databases = new();
    private readonly ILogger<LiteDbManager> _logger;
    private readonly MigrationManager _manager;
    private readonly LiteDbOptions _options;
    private LiteDatabase _commonDatabase;

    public LiteDbManager(ILogger<LiteDbManager> logger, IOptions<LiteDbOptions> options, MigrationManager manager) {
        _logger = logger;
        _manager = manager;
        _options = options.Value;
    }

    public BsonMapper BsonMapper { get; set; }

    private string CreateConnectionString(string identifier = "common") {
        return $"{_options.PathPrefix}{identifier}_{_options.FileSuffix}.db";
    }

    private string GetGuildFileName(ulong guildId) {
        return CreateConnectionString(guildId.ToString());
    }

    public LiteDatabase GetCommonDatabase() {
        lock (_commonLock) {
            _logger.LogTrace("Requesting Common LiteDb");
            return _commonDatabase ??= CreateDatabase(CreateConnectionString());
        }
    }

    public LiteDatabase GetDatabase(ulong guildId) {
        lock (_createLock) {
            _logger.LogTrace("Requesting LiteDb for {guild}", guildId);

            if (!_databases.TryGetValue(guildId, out var database)) {
                _logger.LogTrace("Creating LiteDb for {guild}", guildId);
                database = CreateDatabase(GetGuildFileName(guildId));
                _databases.Add(guildId, database);
            }

            return database;
        }
    }

    private LiteDatabase CreateDatabase(string connectionString) {
        var liteDatabase = new LiteDatabase(connectionString, BsonMapper);

        using (LogContext.PushProperty("db", connectionString)) {
            _manager.Migrate(liteDatabase);
        }

        return liteDatabase;
    }
}