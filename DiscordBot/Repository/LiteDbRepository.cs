using System;
using System.Collections.Generic;
using System.Linq;
using DiscordBotFanatic.Models;
using DiscordBotFanatic.Models.Data;
using LiteDB;
using Player = DiscordBotFanatic.Models.Data.Player;

namespace DiscordBotFanatic.Repository {
    public class LiteDbRepository : IDiscordBotRepository {
        protected const string PlayerCollectionName = "players";
        protected const string GuildConfigurationCollectionName = "guildConfig";
        protected const string GuildEventCollectionName = "guildEvents";
        protected const string GuildCompetitionCollectionName = "guildCompetitions";
        protected const string GuildJobStateCollectionName = "guildJobState";
        protected const string GuildUserCountsCollectionName = "guildUserCounts";
        private readonly object _dbLock = new object();
        private readonly Dictionary<ulong, object> _guildLocks = new Dictionary<ulong, object>();

        public LiteDbRepository(string fileName) {
            FileName = fileName;
            BsonMapper = BsonMapper.Global;
        }

        protected LiteDatabase LiteDatabase { get; set; }
        protected string FileName { get; set; }

        public BsonMapper BsonMapper { get; set; }

        public Player GetPlayerByOsrsAccount(ulong guildId, int womId) {
            lock (GetGuildLock(guildId)) {
                using (LiteDatabase = new LiteDatabase(GetGuildFileName(guildId), BsonMapper)) {
                    return GetPlayerQuery(LiteDatabase)
                        .Where(p => p.CoupledOsrsAccounts.Select(wom => wom.Id).Any(id => id == womId))
                        .FirstOrDefault();
                }
            }
        }

        public Player GetPlayerByOsrsAccount(ulong guildId, string username) {
            username = username.ToLowerInvariant();
            lock (GetGuildLock(guildId)) {
                using (LiteDatabase = new LiteDatabase(GetGuildFileName(guildId), BsonMapper)) {
                    return GetPlayerQuery(LiteDatabase).Where(p =>
                            p.CoupledOsrsAccounts.Select(wom => wom.Username).Any(name => name == username))
                        .FirstOrDefault();
                }
            }
        }

        public Player CoupleOsrsPlayerToGuild(ulong guildId, ulong discordUserId,
            WiseOldManConnector.Models.Output.Player womPlayer) {
            var player = GetPlayerById(guildId, discordUserId);
            if (player == null) {
                player = new Player(guildId, discordUserId);
                player.CoupledOsrsAccounts.Add(womPlayer);


                return InsertPlayerForGuild(guildId, player);
            }

            player.CoupledOsrsAccounts.Add(womPlayer);
            return UpdateOrInsertPlayerForGuild(guildId, player);
        }

        public Player InsertPlayerForGuild(ulong guildId, Player player) {
            player.IsValid();

            lock (GetGuildLock(guildId)) {
                using (LiteDatabase = new LiteDatabase(GetGuildFileName(guildId), BsonMapper)) {
                    var collection = LiteDatabase.GetCollection<Player>(PlayerCollectionName);
                    collection.Insert(player);
                }
            }

            return GetPlayerById(guildId, player.DiscordId);
        }

        public GroupConfig CreateOrUpdateGroupConfig(GroupConfig config) {
            if (config._id == null) {
                return InsertConfig(config);
            }

            config.IsValid();
            lock (GetGuildLock(config.GuildId)) {
                using (LiteDatabase = new LiteDatabase(GetGuildFileName(config.GuildId), BsonMapper)) {
                    var collection = LiteDatabase.GetCollection<GroupConfig>(GuildConfigurationCollectionName);
                    collection.Update(config);
                }
            }

            return GetGroupConfig(config.GuildId);
        }

        public GroupConfig InsertConfig(GroupConfig config) {
            config.IsValid();

            lock (GetGuildLock(config.GuildId)) {
                using (LiteDatabase = new LiteDatabase(GetGuildFileName(config.GuildId), BsonMapper)) {
                    var collection = LiteDatabase.GetCollection<GroupConfig>(GuildConfigurationCollectionName);
                    collection.Insert(config);
                }
            }

            return GetGroupConfig(config.GuildId);
        }

        public GroupConfig GetGroupConfig(ulong guildId) {
            lock (GetGuildLock(guildId)) {
                using (LiteDatabase = new LiteDatabase(GetGuildFileName(guildId), BsonMapper)) {
                    return GetGroupConfigQuery(LiteDatabase).Where(p => p.GuildId == guildId).FirstOrDefault();
                }
            }
        }

        public IEnumerable<Player> GetAllPlayersForGuild(in ulong guildId) {
            lock (GetGuildLock(guildId)) {
                using (LiteDatabase = new LiteDatabase(GetGuildFileName(guildId), BsonMapper)) {
                    return GetPlayerQuery(LiteDatabase).ToList();
                }
            }
        }

        public AutomatedJobState GetAutomatedJobState(ulong guildId) {
            lock (GetGuildLock(guildId)) {
                using (LiteDatabase = new LiteDatabase(GetGuildFileName(guildId), BsonMapper)) {
                    var collection = LiteDatabase.GetCollection<AutomatedJobState>(GuildJobStateCollectionName);

                    if (collection.Count() > 1) {
                        throw new Exception($"Multiple job states!");
                    }

                    return collection.Query().Where(j => j.GuildId == guildId).FirstOrDefault();
                }
            }
        }

        public AutomatedJobState CreateOrUpdateAutomatedJobState(ulong guildId, AutomatedJobState jobState) {
            if (jobState._id == null) {
                return InsertAutomatedJobState(jobState);
            }

            lock (GetGuildLock(guildId)) {
                using (LiteDatabase = new LiteDatabase(GetGuildFileName(guildId), BsonMapper)) {
                    var collection = LiteDatabase.GetCollection<AutomatedJobState>(GuildJobStateCollectionName);
                    collection.Update(jobState);
                }
            }

            return GetAutomatedJobState(jobState.GuildId);
        }

        public UserCountInfo GetCountInfoByUserId(ulong guildId, ulong userId) {
            lock (GetGuildLock(guildId)) {
                using (LiteDatabase = new LiteDatabase(GetGuildFileName(guildId), BsonMapper)) {
                    return GetCountQuery(LiteDatabase).Where(c => c.DiscordId == userId).FirstOrDefault();
                }
            }
        }

        public UserCountInfo UpdateOrInsertUserCountInfoForGuid(ulong guildId, UserCountInfo countInfo) {
            if (countInfo._id == null) {
                return InsertUserCountInfoForGuid(guildId, countInfo);
            }

            countInfo.IsValid();
            lock (GetGuildLock(guildId)) {
                using (LiteDatabase = new LiteDatabase(GetGuildFileName(guildId), BsonMapper)) {
                    var collection = LiteDatabase.GetCollection<UserCountInfo>(GuildUserCountsCollectionName);
                    collection.Update(countInfo);
                }
            }

            return GetCountInfoByUserId(guildId, countInfo.DiscordId);
        }

        public IEnumerable<UserCountInfo> GetAllUserCountInfos(ulong guildId) {
            lock (GetGuildLock(guildId)) {
                using (LiteDatabase = new LiteDatabase(GetGuildFileName(guildId), BsonMapper)) {
                    return GetCountQuery(LiteDatabase).ToList();
                }
            }
        }

        private UserCountInfo InsertUserCountInfoForGuid(ulong guildId, UserCountInfo countInfo) {
            lock (GetGuildLock(guildId)) {
                using (LiteDatabase = new LiteDatabase(GetGuildFileName(guildId), BsonMapper)) {
                    var collection = LiteDatabase.GetCollection<UserCountInfo>(GuildUserCountsCollectionName);
                    collection.Insert(countInfo);
                }
            }
            
            return GetCountInfoByUserId(guildId, countInfo.DiscordId);
        }


        public Player UpdateOrInsertPlayerForGuild(ulong guildId, Player player) {
            if (player._id == null) {
                return InsertPlayerForGuild(guildId, player);
            }

            player.IsValid();
            lock (GetGuildLock(guildId)) {
                using (LiteDatabase = new LiteDatabase(GetGuildFileName(guildId), BsonMapper)) {
                    var collection = LiteDatabase.GetCollection<Player>(PlayerCollectionName);
                    collection.Update(player);
                }
            }

            return GetPlayerById(guildId, player.DiscordId);
        }

        public Player GetPlayerById(ulong guildId, ulong id) {
            lock (GetGuildLock(guildId)) {
                using (LiteDatabase = new LiteDatabase(GetGuildFileName(guildId), BsonMapper)) {
                    return GetPlayerQuery(LiteDatabase).Where(p => p.DiscordId == id).FirstOrDefault();
                }
            }
        }

        public IEnumerable<Player> GetPlayersForGuild(ulong guildId) {
            lock (GetGuildLock(guildId)) {
                using (LiteDatabase = new LiteDatabase(GetGuildFileName(guildId), BsonMapper)) {
                    return GetPlayerQuery(LiteDatabase).ToList();
                }
            }
        }

        private AutomatedJobState InsertAutomatedJobState(AutomatedJobState jobState) {
            lock (GetGuildLock(jobState.GuildId)) {
                using (LiteDatabase = new LiteDatabase(GetGuildFileName(jobState.GuildId))) {
                    var collection = LiteDatabase.GetCollection<AutomatedJobState>(GuildJobStateCollectionName);
                    collection.Insert(jobState);
                }
            }

            return GetAutomatedJobState(jobState.GuildId);
        }

        private string GetGuildFileName(ulong guildId) {
            return $"{guildId}_{FileName}";
        }

        private Object GetGuildLock(ulong guildId) {
            if (_guildLocks.ContainsKey(guildId)) {
                return _guildLocks[guildId];
            }

            object lockObject = new object();
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

        private ILiteQueryable<GroupConfig> GetGroupConfigQuery(LiteDatabase db) {
            var collection = LiteDatabase.GetCollection<GroupConfig>(GuildConfigurationCollectionName);
            return collection.Query();
        }
    }
}