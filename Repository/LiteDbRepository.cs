using System.Collections.Generic;
using DiscordBotFanatic.Models.Data;
using LiteDB;

namespace DiscordBotFanatic.Repository {
    public class LiteDbRepository : IDiscordBotRepository {

        private readonly object _dbLock = new object();
        protected LiteDatabase LiteDatabase { get; set; }
        protected string FileName { get; set; }

        protected const string PlayerCollectionName = "players";

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
                    return collection.Query().Where(x=>x.DiscordId == id).Limit(1).SingleOrDefault();
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
            }else {
                var fromDb = GetPlayerByDiscordId(player.DiscordId);
                if (fromDb != null) {
                    player._id = fromDb._id;
                    UpdatePlayer(player);
                }
                else {
                    InsertPlayer(player);
                }
            }

            return GetPlayerByDiscordId(player.DiscordId);
        }
    }
}