using System.Collections.Generic;
using Discord;
using DiscordBotFanatic.Models.Data;
using LiteDB;

namespace DiscordBotFanatic.Repository {
    public interface IDiscordBotRepository {
        List<Player> GetAllPlayers();
        Player GetPlayerByDiscordId(string id);
        Player GetPlayerByDiscordId(ulong id);
        Player InsertPlayer(Player player);
        Player UpdatePlayer(Player player);
        Player InsertOrUpdatePlayer(Player player);

        bool HasActiveEvent(IGuild guild);
        GuildEvent GetGuildEventById(ObjectId id);
        GuildEvent InsertGuildEvent(GuildEvent guildEvent);
        GuildConfiguration GetGuildConfigurationById(ulong guildId);
        GuildConfiguration UpdateOrInsertGuildConfiguration(GuildConfiguration guildConfiguration);
        GuildConfiguration UpdateGuildConfiguration(GuildConfiguration guildConfiguration);
        GuildConfiguration InsertGuildConfiguration(GuildConfiguration guildConfiguration);
        List<GuildEvent> GetAllGuildEvents(ulong guildId);
        List<GuildEvent> GetAllActiveGuildEvents(ulong guildId);
        GuildEvent UpdateGuildEvent(GuildEvent guildEvent);
    }
}