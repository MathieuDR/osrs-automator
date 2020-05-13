using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using DiscordBotFanatic.Models.Data;
using LiteDB;

namespace DiscordBotFanatic.Repository {
    public class LiteDbRepository : IDiscordBotRepository {
        private readonly object _dbLock = new object();
        protected LiteDatabase LiteDatabase { get; set; }
        protected string FileName { get; set; }

        protected const string PlayerCollectionName = "players";
        protected const string GuildConfigurationCollectionName = "guildConfig";
        protected const string GuildEventCollectionName = "guildEvents";

        public LiteDbRepository(string fileName) {
            FileName = fileName;
        }

        public List<Player> GetAllPlayers() {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<Player>(PlayerCollectionName);
                    return collection.Query().ToList();
                }
            }
        }

        public Player GetPlayerByDiscordId(string id) {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<Player>(PlayerCollectionName);
                    return collection.Query().Where(x => x.DiscordId == id).Limit(1).SingleOrDefault();
                }
            }
        }

        public Player GetPlayerByDiscordId(ulong id) {
            return GetPlayerByDiscordId(id.ToString());
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

        public GuildEvent GetGuildEventById(ObjectId id) {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<GuildEvent>(GuildEventCollectionName);
                    return collection.Query().Where(x => x._id == id).SingleOrDefault();
                }
            }
        }

        public GuildEvent InsertGuildEvent(GuildEvent guildEvent) {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<GuildEvent>(GuildEventCollectionName);
                    collection.Insert(guildEvent);
                }
            }

            return GetGuildEventById(guildEvent._id);
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

        public List<GuildEvent> GetAllGuildEvents(ulong guildId) {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<GuildEvent>(GuildEventCollectionName);
                    return collection.Query().Where(x=>x.GuildId == guildId).ToList();
                }
            }
        }

        public List<GuildEvent> GetAllActiveGuildEvents(ulong guildId) {
            var allEvents = GetAllGuildEvents(guildId).ToList();
            var result = new List<GuildEvent>();
            foreach (GuildEvent guildEvent in allEvents) {
                if (guildEvent.EndTime == DateTime.MinValue) {
                    result.Add(guildEvent);
                }

                if (guildEvent.EndTime >= DateTime.Now) {
                    result.Add(guildEvent);
                }
            }

            return result;
            //return allEvents.Where(x => x.EndTime == DateTime.MinValue || x.EndTime >= DateTime.Now).ToList();
        }

        public GuildEvent UpdateGuildEvent(GuildEvent guildEvent) {
            lock (_dbLock) {
                using (LiteDatabase = new LiteDatabase(FileName)) {
                    var collection = LiteDatabase.GetCollection<GuildEvent>(GuildEventCollectionName);
                    collection.Update(guildEvent);
                }
            }

            return GetGuildEventById(guildEvent._id);
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