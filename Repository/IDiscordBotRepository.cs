using System.Collections.Generic;
using DiscordBotFanatic.Models.Data;

namespace DiscordBotFanatic.Repository {
    public interface IDiscordBotRepository {
        List<Player> GetAllPlayers();
        Player GetPlayerByDiscordId(string id);
        Player GetPlayerByDiscordId(ulong id);
        Player InsertPlayer(Player player);
        Player UpdatePlayer(Player player);
        Player InsertOrUpdatePlayer(Player player);
    }
}