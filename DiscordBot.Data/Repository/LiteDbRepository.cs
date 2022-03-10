using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Data.Configuration;
using DiscordBot.Common.Models.Data.Counting;
using DiscordBot.Common.Models.Data.PlayerManagement;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository.Migrations;
using LiteDB;
using Serilog;
using Serilog.Context;

namespace DiscordBot.Data.Repository;

internal class LiteDbRepository : IDiscordBotRepository {
    protected const string PlayerCollectionName = "players";
    protected const string GuildConfigurationCollectionName = "guildConfig";
    protected const string GuildEventCollectionName = "guildEvents";
    protected const string GuildCompetitionCollectionName = "guildCompetitions";
    protected const string GuildJobStateCollectionName = "guildJobState";
    protected const string GuildUserCountsCollectionName = "guildUserCounts";
    private readonly object _dbLock = new();
    private readonly Dictionary<ulong, object> _guildLocks = new();
    private readonly ILogger _logger;
    private readonly MigrationManager _migrationManager;

    public LiteDbRepository(ILogger logger, string fileName, MigrationManager migrationManager) {
        _logger = logger;
        _migrationManager = migrationManager;
        FileName = fileName;
        BsonMapper = BsonMapper.Global;
    }

    protected LiteDatabase LiteDatabase { get; set; }
    protected string FileName { get; set; }

    public BsonMapper BsonMapper { get; set; }

    public Player GetPlayerByOsrsAccount(DiscordGuildId guildId, int womId) {
        lock (GetGuildLock(guildId)) {
            using (LiteDatabase = GetDatabase(guildId)) {
                return GetPlayerQuery(LiteDatabase)
                    .Where(p => p.CoupledOsrsAccounts.Select(wom => wom.Id).Any(id => id == womId))
                    .FirstOrDefault();
            }
        }
    }

    public Player GetPlayerByOsrsAccount(DiscordGuildId guildId, string username) {
        username = username.ToLowerInvariant();
        lock (GetGuildLock(guildId)) {
            using (LiteDatabase = GetDatabase(guildId)) {
                return GetPlayerQuery(LiteDatabase).Where(p =>
                        p.CoupledOsrsAccounts.Select(wom => wom.Username).Any(name => name == username))
                    .FirstOrDefault();
            }
        }
    }

    public Player CoupleOsrsPlayerToGuild(DiscordGuildId guildId, DiscordUserId discordUserId, WiseOldManConnector.Models.Output.Player womPlayer) {
        var player = GetPlayerById(guildId, discordUserId);
        if (player == null) {
            player = new Player(guildId, discordUserId);
            player.CoupledOsrsAccounts.Add(womPlayer);


            return InsertPlayerForGuild(guildId, player);
        }

        player.CoupledOsrsAccounts.Add(womPlayer);
        return UpdateOrInsertPlayerForGuild(guildId, player);
    }

    public Player InsertPlayerForGuild(DiscordGuildId guildId, Player player) {
        player.IsValid();

        lock (GetGuildLock(guildId)) {
            using (LiteDatabase = GetDatabase(guildId)) {
                var collection = LiteDatabase.GetCollection<Player>(PlayerCollectionName);
                collection.Insert(player);
            }
        }

        return GetPlayerById(guildId, player.DiscordUserId);
    }

    public GuildConfig CreateOrUpdateGroupConfig(GuildConfig config) {
        if (config._id == null) {
            return InsertConfig(config);
        }

        config.IsValid();
        lock (GetGuildLock(config.GuildId)) {
            using (LiteDatabase = GetDatabase(config.GuildId)) {
                var collection = LiteDatabase.GetCollection<GuildConfig>(GuildConfigurationCollectionName);
                collection.Update(config);
            }
        }

        return GetGroupConfig(config.GuildId);
    }

    public GuildConfig InsertConfig(GuildConfig config) {
        config.IsValid();

        lock (GetGuildLock(config.GuildId)) {
            using (LiteDatabase = GetDatabase(config.GuildId)) {
                var collection = LiteDatabase.GetCollection<GuildConfig>(GuildConfigurationCollectionName);
                collection.Insert(config);
            }
        }

        return GetGroupConfig(config.GuildId);
    }

    public GuildConfig GetGroupConfig(DiscordGuildId guildId) {
        lock (GetGuildLock(guildId)) {
            using (LiteDatabase = GetDatabase(guildId)) {
                return GetGroupConfigQuery(LiteDatabase).Where(p => p.GuildId == guildId).FirstOrDefault();
            }
        }
    }

    public IEnumerable<Player> GetAllPlayersForGuild(in DiscordGuildId guildId) {
        lock (GetGuildLock(guildId)) {
            using (LiteDatabase = GetDatabase(guildId)) {
                return GetPlayerQuery(LiteDatabase).ToList();
            }
        }
    }

    public AutomatedJobState GetAutomatedJobState(DiscordGuildId guildId) {
        lock (GetGuildLock(guildId)) {
            using (LiteDatabase = GetDatabase(guildId)) {
                var collection = LiteDatabase.GetCollection<AutomatedJobState>(GuildJobStateCollectionName);

                if (collection.Count() > 1) {
                    throw new Exception("Multiple job states!");
                }

                return collection.Query().Where(j => j.GuildId == guildId).FirstOrDefault();
            }
        }
    }

    public AutomatedJobState CreateOrUpdateAutomatedJobState(DiscordGuildId guildId, AutomatedJobState jobState) {
        if (jobState._id == null) {
            return InsertAutomatedJobState(jobState);
        }

        lock (GetGuildLock(guildId)) {
            using (LiteDatabase = GetDatabase(guildId)) {
                var collection = LiteDatabase.GetCollection<AutomatedJobState>(GuildJobStateCollectionName);
                collection.Update(jobState);
            }
        }

        return GetAutomatedJobState(jobState.GuildId);
    }


    public UserCountInfo GetCountInfoByUserId(DiscordGuildId guildId, DiscordUserId userId) {
        lock (GetGuildLock(guildId)) {
            using (LiteDatabase = GetDatabase(guildId)) {
                return GetCountQuery(LiteDatabase).Where(c => c.DiscordId == userId).FirstOrDefault();
            }
        }
    }

    public UserCountInfo UpdateOrInsertUserCountInfoForGuid(DiscordGuildId guildId, UserCountInfo countInfo) {
        if (countInfo._id == null) {
            return InsertUserCountInfoForGuid(guildId, countInfo);
        }

        countInfo.IsValid();
        lock (GetGuildLock(guildId)) {
            using (LiteDatabase = GetDatabase(guildId)) {
                var collection = LiteDatabase.GetCollection<UserCountInfo>(GuildUserCountsCollectionName);
                collection.Update(countInfo);
            }
        }

        return GetCountInfoByUserId(guildId, countInfo.DiscordId);
    }

    public IEnumerable<UserCountInfo> GetAllUserCountInfos(DiscordGuildId guildId) {
        lock (GetGuildLock(guildId)) {
            using (LiteDatabase = GetDatabase(guildId)) {
                return GetCountQuery(LiteDatabase).ToList();
            }
        }
    }

    public Player UpdateOrInsertPlayerForGuild(DiscordGuildId guildId, Player player) {
        if (player._id == null) {
            return InsertPlayerForGuild(guildId, player);
        }

        player.IsValid();
        lock (GetGuildLock(guildId)) {
            using (LiteDatabase = GetDatabase(guildId)) {
                var collection = LiteDatabase.GetCollection<Player>(PlayerCollectionName);
                collection.Update(player);
            }
        }

        return GetPlayerById(guildId, player.DiscordUserId);
    }

    public Player GetPlayerById(DiscordGuildId guildId, DiscordUserId id) {
        lock (GetGuildLock(guildId)) {
            using (LiteDatabase = GetDatabase(guildId)) {
                return GetPlayerQuery(LiteDatabase).Where(p => p.DiscordUserId == id).FirstOrDefault();
            }
        }
    }

    public IEnumerable<Player> GetPlayersForGuild(DiscordGuildId guildId) {
        lock (GetGuildLock(guildId)) {
            using (LiteDatabase = GetDatabase(guildId)) {
                return GetPlayerQuery(LiteDatabase).ToList();
            }
        }
    }

    private AutomatedJobState InsertAutomatedJobState(AutomatedJobState jobState) {
        lock (GetGuildLock(jobState.GuildId)) {
            using (LiteDatabase = GetDatabase(jobState.GuildId)) {
                var collection = LiteDatabase.GetCollection<AutomatedJobState>(GuildJobStateCollectionName);
                collection.Insert(jobState);
            }
        }

        return GetAutomatedJobState(jobState.GuildId);
    }


    private LiteDatabase GetDatabase(DiscordGuildId guildId) {
        return GetDatabase(GetGuildFileName(guildId));
    }

    private LiteDatabase GetDatabase(string connectionString) {
        var liteDatabase = new LiteDatabase(connectionString, BsonMapper);

        using (LogContext.PushProperty("db", connectionString)) {
            _migrationManager.Migrate(liteDatabase);
        }

        return liteDatabase;
    }

    private UserCountInfo InsertUserCountInfoForGuid(DiscordGuildId guildId, UserCountInfo countInfo) {
        lock (GetGuildLock(guildId)) {
            using (LiteDatabase = GetDatabase(guildId)) {
                var collection = LiteDatabase.GetCollection<UserCountInfo>(GuildUserCountsCollectionName);
                collection.Insert(countInfo);
            }
        }

        return GetCountInfoByUserId(guildId, countInfo.DiscordId);
    }

    private string GetGuildFileName(DiscordGuildId guildId) {
        return $"{guildId}_{FileName}";
    }

    private object GetGuildLock(DiscordGuildId guildId) {
        if (_guildLocks.ContainsKey(guildId)) {
            return _guildLocks[guildId];
        }

        var lockObject = new object();
        _guildLocks.Add(guildId, lockObject);
        return lockObject;
    }

    private ILiteQueryable<Player> GetPlayerQuery(LiteDatabase db) {
        var collection = LiteDatabase.GetCollection<Player>(PlayerCollectionName);
        return collection.Query();
    }

    private ILiteQueryable<UserCountInfo> GetCountQuery(LiteDatabase db) {
        var collection = LiteDatabase.GetCollection<UserCountInfo>(GuildUserCountsCollectionName);
        return collection.Query();
    }

    private ILiteQueryable<GuildConfig> GetGroupConfigQuery(LiteDatabase db) {
        var collection = LiteDatabase.GetCollection<GuildConfig>(GuildConfigurationCollectionName);
        return collection.Query();
    }
}
