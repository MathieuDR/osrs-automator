using System.Collections.Generic;
using Discord;
using DiscordBotFanatic.Models.Data;
using LiteDB;

namespace DiscordBotFanatic.Repository {
    public interface IDiscordBotRepository {
        List<Player> GetAllPlayers();
        Player GetPlayerByDiscordId(ulong id);
        Player InsertPlayer(Player player);
        Player UpdatePlayer(Player player);
        Player InsertOrUpdatePlayer(Player player);

        bool HasActiveEvent(IGuild guild);
        GuildCustomEvent GetGuildEventById(ObjectId id);
        GuildCustomEvent InsertGuildEvent(GuildCustomEvent guildCustomEvent);
        GuildConfiguration GetGuildConfigurationById(ulong guildId);
        GuildConfiguration UpdateOrInsertGuildConfiguration(GuildConfiguration guildConfiguration);
        GuildConfiguration UpdateGuildConfiguration(GuildConfiguration guildConfiguration);
        GuildConfiguration InsertGuildConfiguration(GuildConfiguration guildConfiguration);
        List<GuildCustomEvent> GetAllGuildEvents(ulong guildId);
        List<GuildCustomEvent> GetAllActiveGuildEvents(ulong guildId);
        GuildCustomEvent UpdateGuildEvent(GuildCustomEvent guildCustomEvent);


        
        GuildCompetition InsertGuildCompetition(GuildCompetition guildCompetition);
        GuildCompetition GetGuildCompetitionById(ObjectId id);
        GuildCompetition GetGuildCompetitionById(int id);
        List<GuildCompetition> GetAllGuildCompetitions(ulong guildId);
        List<GuildCompetition> GetAllActiveGuildCompetitions(ulong guildId);
        GuildCompetition UpdateGuildCompetition(GuildCompetition guildCompetition);
    }
}