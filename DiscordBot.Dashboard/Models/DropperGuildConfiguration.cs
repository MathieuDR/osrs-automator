using DiscordBot.Common.Identities;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Dashboard.Models; 

public class DropperGuildConfigurationModel {
    public DiscordGuildId GuildId { get; set; }
    public bool IsEnabled { get; set; }
    public List<DiscordUserId> DisabledUsers { get; set; }
    public DropperConfigurationRequest BaseConfiguration { get; set; }
    public Dictionary<DiscordChannelId, List<DropperConfigurationRequest>> ChannelConfigurations { get; init; }
}

public class DropperConfigurationRequest {
    public bool IsEnabled { get; set; }
    public List<DiscordUserId> WhiteListedUsers { get; set; }
    public List<DiscordUserId> BlackListedUsers { get; set; }
    public List<string> WhiteListedItems { get; init; }
    public List<string> BlackListedItems { get; init; }
    public List<string> WhiteListedSources { get; init; }
    public List<string> BlackListedSources { get; init; }

    public int MinimumGeValue { get; init; }
    public int MinimumHaValue { get; init; }
    public bool RequireBothGeAndHaValue { get; init; }

    public List<PlayerBuild> WhiteListedPlayerType { get; init; }
    public List<PlayerBuild> BlackListedPlayerType { get; init; }

    public bool AlwaysSendCollectionLogItems { get; init; }
}