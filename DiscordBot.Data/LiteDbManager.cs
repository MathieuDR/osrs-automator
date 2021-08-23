using System.Collections.Generic;
using DiscordBot.Data.Repository.Migrations;
using LiteDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace DiscordBot.Data {
    public class LiteDbManager {
        private object createLock = new object();
        private readonly ILogger<LiteDbManager> _logger;
        private readonly MigrationManager _manager;
        private readonly LiteDbOptions _options;
        private readonly Dictionary<ulong, LiteDatabase> _databases = new Dictionary<ulong, LiteDatabase>();
        public BsonMapper BsonMapper { get; set; }

        public LiteDbManager(ILogger<LiteDbManager> logger, IOptions<LiteDbOptions> options,MigrationManager manager) {
            _logger = logger;
            _manager = manager;
            _options = options.Value;
        }
        private string GetGuildFileName(ulong guildId) {
            return $"{guildId}_{_options.FileName}";
        }
        
        public LiteDatabase GetDatabase(ulong guildId) {
            lock (createLock) {
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

    public class LiteDbOptions {
        public string FileName { get; set; }
    }
}
