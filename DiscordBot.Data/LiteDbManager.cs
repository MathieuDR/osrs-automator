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

public class LiteDbManager {
    private sealed class Entry {
        public LiteDatabase Db;
        public int RefCount;
    }

    // LiteDB's engine constructor is not safe to call from multiple threads at once (it touches
    // process-wide static state independent of which file is being opened), so every LiteDatabase
    // construction across every LiteDbManager instance is serialized through this one static lock.
    private static readonly object _constructionLock = new();

    private readonly object _gate = new();
    private readonly Dictionary<string, Entry> _open = new();
    private readonly Dictionary<string, object> _fileLocks = new();
    private readonly ILogger<LiteDbManager> _logger;
    private readonly MigrationManager _manager;
    private readonly LiteDbOptions _options;

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

    public int OpenDatabaseCount {
        get {
            lock (_gate) {
                return _open.Count;
            }
        }
    }

    private string CreateConnectionString(string identifier = "common") {
        return $"{_options.PathPrefix}{identifier}_{_options.FileSuffix}.db";
    }

    private string GetGuildFileName(DiscordGuildId guildId) {
        return CreateConnectionString(guildId.ToString());
    }

    public DatabaseLease Lease(DiscordGuildId guildId) {
        _logger.LogTrace("Requesting LiteDb lease for {guild}", guildId);
        return LeaseFile(GetGuildFileName(guildId));
    }

    public DatabaseLease LeaseCommon() {
        _logger.LogTrace("Requesting Common LiteDb lease");
        return LeaseFile(CreateConnectionString());
    }

    // _gate only ever guards quick, in-memory dictionary bookkeeping. The (potentially slow) I/O of
    // opening or disposing a LiteDatabase happens under a lock scoped to that one file's path, so
    // that different guilds' databases never block each other, while still preventing the race
    // where a Lease() for a path slips in between another thread dropping the entry from _open and
    // that thread's Dispose() completing - LiteDB does not tolerate two engines on one file at once.
    private object GetFileLock(string path) {
        lock (_gate) {
            if (!_fileLocks.TryGetValue(path, out var fileLock)) {
                fileLock = new object();
                _fileLocks[path] = fileLock;
            }

            return fileLock;
        }
    }

    private DatabaseLease LeaseFile(string path) {
        lock (GetFileLock(path)) {
            Entry entry;
            lock (_gate) {
                _open.TryGetValue(path, out entry);
            }

            if (entry is null) {
                entry = new Entry { Db = CreateDatabase(path) };
                lock (_gate) {
                    _open[path] = entry;
                }
            }

            lock (_gate) {
                entry.RefCount++;
            }

            return new DatabaseLease(this, path, entry.Db);
        }
    }

    internal void Release(string path) {
        lock (GetFileLock(path)) {
            Entry entry;
            lock (_gate) {
                if (!_open.TryGetValue(path, out entry)) {
                    return;
                }

                entry.RefCount--;
            }

            if (entry.RefCount <= 0 && _options.CloseWhenUnused) {
                entry.Db.Dispose();
                lock (_gate) {
                    _open.Remove(path);
                }
            }
        }
    }

    private LiteDatabase CreateDatabase(string connectionString) {
        CreateDirectory(connectionString);

        LiteDatabase liteDatabase;
        lock (_constructionLock) {
            liteDatabase = new LiteDatabase(connectionString, BsonMapper);
        }

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

    public void DisposeAll() {
        _logger.LogInformation("Disposing all open databases");
        List<LiteDatabase> toDispose;
        lock (_gate) {
            toDispose = _open.Values.Select(e => e.Db).ToList();
            _open.Clear();
        }

        foreach (var db in toDispose) {
            db.Dispose();
        }
    }
}
