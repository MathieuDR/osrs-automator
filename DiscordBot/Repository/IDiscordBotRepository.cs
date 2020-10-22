using System.Collections.Generic;
using Discord;
using DiscordBotFanatic.Models.Data;
using LiteDB;
using WiseOldManConnector.Models.Output;
using OsrsPlayer = WiseOldManConnector.Models.Output.Player;
using Player = DiscordBotFanatic.Models.Data.Player;

namespace DiscordBotFanatic.Repository {
    public interface IDiscordBotRepository {
        Player GetPlayerByOsrsAccount(ulong guildId, int womId);
        Player GetPlayerByOsrsAccount(ulong guildId, string username);
        Player CoupleOsrsPlayerToGuild(ulong guildId, ulong discordUserId, OsrsPlayer womPlayer);
        Player UpdateOrInsertPlayerForGuild(ulong guildId, Player player);

        Player GetPlayerById(ulong guildId, ulong id);
        Player InsertPlayerForGuild(ulong guildId, Player player);
        GroupConfig UpdateOrInsertGroupConfig(GroupConfig config);
        GroupConfig InsertConfig(GroupConfig config);
        GroupConfig GetGroupConfig(ulong guildId);
        IEnumerable<Player> GetAllPlayersForGuild(in ulong guildId);
    }
}