using System.Collections.Generic;
using DiscordBot.Data.Configuration;
using DiscordBot.Data.Repository.Migrations;
using LiteDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace DiscordBot.Data {
    public class LiteDbManager {
        private readonly object _createLock = new object();
        private readonly object _commonLock = new object();
        private readonly ILogger<LiteDbManager> _logger;
        private readonly MigrationManager _manager;
        private readonly LiteDbOptions _options;
        private readonly Dictionary<ulong, LiteDatabase> _databases = new Dictionary<ulong, LiteDatabase>();
        private LiteDatabase _commonDatabase;
        public BsonMapper BsonMapper { get; set; }

        public LiteDbManager(ILogger<LiteDbManager> logger, IOptions<LiteDbOptions> options,MigrationManager manager) {
            _logger = logger;
            _manager = manager;
            _options = options.Value;
        }

        private string CreateConnectionString(string identifier = "common") => $"{_options.PathPrefix}{identifier}_{_options.FileSuffix}.db";
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
                
                if (!_databases.TryGetValue(guildId, out LiteDatabase database)) {
                    _logger.LogTrace("Creating LiteDb for {guild}", guildId);
                    database = CreateDatabase(GetGuildFileName(guildId));
                    _databases.Add(guildId,database);
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
}
