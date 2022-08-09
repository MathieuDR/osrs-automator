using System.Collections;
using System.Diagnostics;
using DiscordBot.Common.Models.Data.Graveyard;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Data.Configuration;
using DiscordBot.Data.Repository.Migrations;
using LiteDB;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace DiscordBot.Data;

public class LiteDbManager: IDisposable {
    private readonly object _commonLock = new();
    private readonly object _createLock = new();
    private readonly Dictionary<DiscordGuildId, LiteDatabase> _databases = new();
    private readonly ILogger<LiteDbManager> _logger;
    private readonly MigrationManager _manager;
    private readonly LiteDbOptions _options;
    private LiteDatabase _commonDatabase;

    public LiteDbManager(ILogger<LiteDbManager> logger, IOptions<LiteDbOptions> options, MigrationManager manager) {
        _logger = logger;
        _manager = manager;
        _options = options.Value;
        
        AddMappers();
    }

    private void AddMappers() {
        BsonMapper = BsonMapper.Global;
        BsonMapper.RegisterType(id => id.Value, bson => new DiscordUserId(bson.AsInt64));
        BsonMapper.RegisterType(id => id.Value, bson => new DiscordGuildId(bson.AsInt64));
        BsonMapper.RegisterType(id => id.Value, bson => new DiscordChannelId(bson.AsInt64));
        BsonMapper.RegisterType(id => id.Value, bson => new DiscordMessageId(bson.AsInt64));
        BsonMapper.RegisterType(id => id.Value, bson => new DiscordRoleId(bson.AsInt64));
        
        AddDictMapper<DiscordRoleId, AuthorizationRoles>(x=> new DiscordRoleId(x));
        AddDictMapper<DiscordUserId, AuthorizationRoles>(x=> new DiscordUserId(x));
        AddDictMapper<DiscordUserId, List<Shame>>(x=> new DiscordUserId(x));
        AddDictMapper<DiscordUserId, EndpointId>(x=> new DiscordUserId(x));
    }
    
    public static void AddDictMapper<TIdentity, TObject>(Func<ulong, TIdentity> ctor) where TIdentity : new() {
        BsonMapper.Global.RegisterType(
            dictionary => {
                var bsonDocument = new BsonDocument();
                foreach (TIdentity key in dictionary.Keys as IEnumerable) {
                    var obj = dictionary[key];
                    var name = key.ToString();
                    Debug.Assert(name != null, nameof(name) + " != null");
                    bsonDocument[name] = BsonMapper.Global.Serialize(typeof(TObject), obj);
                }

                return bsonDocument;
            }, value => {
                var dict = new Dictionary<TIdentity, TObject>();
                foreach (var element in value.AsDocument.GetElements()) {
                    var userId = ulong.Parse(element.Key);
                    var obj = BsonMapper.Global.Deserialize<TObject>(element.Value);
                    dict.Add(ctor(userId), obj);
                }

                return dict;
            });
    }

    public BsonMapper BsonMapper { get; set; }

    private string CreateConnectionString(string identifier = "common") {
        return $"{_options.PathPrefix}{identifier}_{_options.FileSuffix}.db";
    }

    private string GetGuildFileName(DiscordGuildId guildId) {
        return CreateConnectionString(guildId.ToString());
    }

    public LiteDatabase GetCommonDatabase() {
        lock (_commonLock) {
            _logger.LogTrace("Requesting Common LiteDb");
            return _commonDatabase ??= CreateDatabase(CreateConnectionString());
        }
    }

    public LiteDatabase GetDatabase(DiscordGuildId guildId) {
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
        CreateDirectory(connectionString);
        var liteDatabase = new LiteDatabase(connectionString, BsonMapper);

        using (LogContext.PushProperty("db", connectionString)) {
            _manager.Migrate(liteDatabase);
        }

        return liteDatabase;
    }

    private static void CreateDirectory(string connectionString) {
        var path = Path.GetDirectoryName(connectionString);
        if (!string.IsNullOrWhiteSpace(path) && !Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }
    }

    public void ClearDb() {
        _commonDatabase?.Dispose();
        _commonDatabase = null;
        foreach (var liteDatabase in _databases) {
            liteDatabase.Value.Dispose();
        }
        
        _databases.Clear();
    }

    public void Dispose() {
        _logger.LogInformation("Disposing");
        ClearDb();
    }
}
