using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Data.Configuration;
using DiscordBot.Common.Models.Data.Counting;
using DiscordBot.Common.Models.Data.PlayerManagement;
using OsrsPlayer = WiseOldManConnector.Models.Output.Player;

namespace DiscordBot.Data.Interfaces;

public interface IDiscordBotRepository {
    Player GetPlayerByOsrsAccount(ulong guildId, int womId);
    Player GetPlayerByOsrsAccount(ulong guildId, string username);
    Player CoupleOsrsPlayerToGuild(ulong guildId, ulong discordUserId, OsrsPlayer womPlayer);
    Player UpdateOrInsertPlayerForGuild(ulong guildId, Player player);

    Player GetPlayerById(ulong guildId, ulong id);
    Player InsertPlayerForGuild(ulong guildId, Player player);
    GuildConfig CreateOrUpdateGroupConfig(GuildConfig config);
    GuildConfig InsertConfig(GuildConfig config);
    GuildConfig GetGroupConfig(ulong guildId);
    IEnumerable<Player> GetAllPlayersForGuild(in ulong guildId);

    AutomatedJobState GetAutomatedJobState(ulong guildId);
    AutomatedJobState CreateOrUpdateAutomatedJobState(ulong guildId, AutomatedJobState jobState);

    UserCountInfo GetCountInfoByUserId(ulong guildId, ulong userId);

    UserCountInfo UpdateOrInsertUserCountInfoForGuid(ulong guildId, UserCountInfo countInfo);
    IEnumerable<UserCountInfo> GetAllUserCountInfos(ulong guildId);
}
