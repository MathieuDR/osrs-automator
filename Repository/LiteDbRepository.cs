using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using DiscordBotFanatic.Models.Data;
using LiteDB;

namespace DiscordBotFanatic.Repository {
    public class LiteDbRepository : IDiscordBotRepository {
        protected const string PlayerCollectionName = "players";
        protected const string GuildConfigurationCollectionName = "guildConfig";
        protected const string GuildEventCollectionName = "guildEvents";
        protected const string GuildCompetitionCollectionName= "guildCompetitions";
        protected const string GuildCompetitionFixesCollectionName = "guildCompetitionFix";
        private readonly object _dbLock = new object();

        public LiteDbRepository(string fileName) {
            FileName = fileName;
        }

        protected LiteDatabase LiteDatabase { get; set; }
        protected string FileName { get; set; }

        public List<Player> GetAllPlayers() {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<Player>(PlayerCollectionName);
                    return collection.Query().ToList();
                }
            }
        }

        public Player GetPlayerByDiscordId(ulong id) {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<Player>(PlayerCollectionName);
                    return collection.Query().Where(x => x.DiscordId == id).Limit(1).SingleOrDefault();
                }
            }
        }

        public Player InsertPlayer(Player player) {
            player.IsValid();

            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<Player>(PlayerCollectionName);
                    collection.Insert(player);
                }
            }

            return GetPlayerByDiscordId(player.DiscordId);
        }

        public Player UpdatePlayer(Player player) {
            player.IsValid();

            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<Player>(PlayerCollectionName);
                    collection.Update(player);
                }
            }

            return GetPlayerByDiscordId(player.DiscordId);
        }

        public Player InsertOrUpdatePlayer(Player player) {
            player.IsValid();

            if (player._id != null) {
                UpdatePlayer(player);
            } else {
                var fromDb = GetPlayerByDiscordId(player.DiscordId);
                if (fromDb != null) {
                    player._id = fromDb._id;
                    UpdatePlayer(player);
                } else {
                    InsertPlayer(player);
                }
            }

            return GetPlayerByDiscordId(player.DiscordId);
        }

        public bool HasActiveEvent(IGuild guild) {
            return GetAllActiveGuildEvents(guild.Id).Any();
        }

        public GuildCustomEvent GetGuildEventById(ObjectId id) {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<GuildCustomEvent>(GuildEventCollectionName);
                    return collection.Query().Where(x => x._id == id).SingleOrDefault();
                }
            }
        }

        public GuildCustomEvent InsertGuildEvent(GuildCustomEvent guildCustomEvent) {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<GuildCustomEvent>(GuildEventCollectionName);
                    collection.Insert(guildCustomEvent);
                }
            }

            return GetGuildEventById(guildCustomEvent._id);
        }

        public GuildConfiguration GetGuildConfigurationById(ulong guildId) {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<GuildConfiguration>(GuildConfigurationCollectionName);
                    return collection.Query().Where(x => x.GuildId == guildId).SingleOrDefault();
                }
            }
        }

        public GuildConfiguration UpdateOrInsertGuildConfiguration(GuildConfiguration guildConfiguration) {
            if (guildConfiguration._id != null) {
                UpdateGuildConfiguration(guildConfiguration);
            } else {
                var fromDb = GetGuildConfigurationById(guildConfiguration.GuildId);
                if (fromDb != null) {
                    guildConfiguration._id = fromDb._id;
                    UpdateGuildConfiguration(guildConfiguration);
                } else {
                    InsertGuildConfiguration(guildConfiguration);
                }
            }

            return GetGuildConfigurationById(guildConfiguration.GuildId);
        }

        public GuildConfiguration InsertGuildConfiguration(GuildConfiguration guildConfiguration) {
            //player.IsValid();

            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<GuildConfiguration>(GuildConfigurationCollectionName);
                    collection.Insert(guildConfiguration);
                }
            }

            return GetGuildConfigurationById(guildConfiguration.GuildId);
        }

        public List<GuildCustomEvent> GetAllGuildEvents(ulong guildId) {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<GuildCustomEvent>(GuildEventCollectionName);
                    return collection.Query().Where(x => x.GuildId == guildId).ToList();
                }
            }
        }

        public List<GuildCustomEvent> GetAllActiveGuildEvents(ulong guildId) {
            var allEvents = GetAllGuildEvents(guildId).ToList();
            var result = new List<GuildCustomEvent>();
            foreach (GuildCustomEvent guildEvent in allEvents) {
                if (guildEvent.EndTime == DateTime.MinValue) {
                    result.Add(guildEvent);
                }

                if (guildEvent.EndTime >= DateTime.Now) {
                    result.Add(guildEvent);
                }
            }

            return result;
        }

        public GuildCustomEvent UpdateGuildEvent(GuildCustomEvent guildCustomEvent) {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<GuildCustomEvent>(GuildEventCollectionName);
                    collection.Update(guildCustomEvent);
                }
            }

            return GetGuildEventById(guildCustomEvent._id);
        }

        public GuildCompetition InsertGuildCompetition(GuildCompetition guildCompetition) {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<GuildCompetition>(GuildCompetitionCollectionName);
                    collection.Insert(guildCompetition);
                }
            }

            return GetGuildCompetitionById(guildCompetition._id);
        }

        public GuildCompetition GetGuildCompetitionById(ObjectId id) {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<GuildCompetition>(GuildCompetitionCollectionName);
                    return collection.Query().Where(x => x._id == id && x.EndTime >= DateTime.UtcNow).SingleOrDefault();
                }
            }
        }

        public GuildCompetition GetGuildCompetitionById(int id) {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<GuildCompetition>(GuildCompetitionCollectionName);
                    return collection.Query().Where(x => x.Id == id && x.EndTime >= DateTime.UtcNow).SingleOrDefault();
                }
            }
        }

        public List<GuildCompetition> GetAllGuildCompetitions(ulong guildId) {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<GuildCompetition>(GuildCompetitionCollectionName);
                    return collection.Query().Where(x => x.GuildId == guildId ).ToList();
                }
            }
        }

        public List<GuildCompetition> GetAllActiveGuildCompetitions(ulong guildId) {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<GuildCompetition>(GuildCompetitionCollectionName);
                    return collection.Query().Where(x => x.GuildId == guildId && x.EndTime >= DateTime.UtcNow).ToList();
                }
            }
        }

        public GuildCompetition UpdateGuildCompetition(GuildCompetition guildCompetition) {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<GuildCompetition>(GuildCompetitionCollectionName);
                    collection.Update(guildCompetition);
                }
            }

            return GetGuildCompetitionById(guildCompetition._id);
        }

        public GuildConfiguration UpdateGuildConfiguration(GuildConfiguration guildConfiguration) {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<GuildConfiguration>(GuildConfigurationCollectionName);
                    collection.Update(guildConfiguration);
                }
            }

            return GetGuildConfigurationById(guildConfiguration.GuildId);
        }
    }
}