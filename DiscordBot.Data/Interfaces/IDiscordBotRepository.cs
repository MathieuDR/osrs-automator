using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Data.Configuration;
using DiscordBot.Common.Models.Data.Counting;
using DiscordBot.Common.Models.Data.PlayerManagement;
using OsrsPlayer = WiseOldManConnector.Models.Output.Player;

namespace DiscordBot.Data.Interfaces;

public interface IDiscordBotRepository {
    Player GetPlayerByOsrsAccount(DiscordGuildId guildId, int womId);
    Player GetPlayerByOsrsAccount(DiscordGuildId guildId, string username);
    Player CoupleOsrsPlayerToGuild(DiscordGuildId guildId, DiscordUserId discordUserId, OsrsPlayer womPlayer);
    Player UpdateOrInsertPlayerForGuild(DiscordGuildId guildId, Player player);

    Player GetPlayerById(DiscordGuildId guildId, DiscordUserId id);
    Player InsertPlayerForGuild(DiscordGuildId guildId, Player player);
    GuildConfig CreateOrUpdateGroupConfig(GuildConfig config);
    GuildConfig InsertConfig(GuildConfig config);
    GuildConfig GetGroupConfig(DiscordGuildId guildId);
    IEnumerable<Player> GetAllPlayersForGuild(in DiscordGuildId guildId);

    AutomatedJobState GetAutomatedJobState(DiscordGuildId guildId);
    AutomatedJobState CreateOrUpdateAutomatedJobState(DiscordGuildId guildId, AutomatedJobState jobState);

    UserCountInfo GetCountInfoByUserId(DiscordGuildId guildId, DiscordUserId userId);

    UserCountInfo UpdateOrInsertUserCountInfoForGuid(DiscordGuildId guildId, UserCountInfo countInfo);
    IEnumerable<UserCountInfo> GetAllUserCountInfos(DiscordGuildId guildId);
}
