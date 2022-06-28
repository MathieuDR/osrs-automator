using DiscordBot.Common.Identities;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Dashboard.Models; 

public class DropperGuildConfigurationModel {
    public bool IsEnabled { get; set; }
    public List<DiscordUserId> DisabledUsers { get; set; }
    public DropperConfigurationRequest BaseConfiguration { get; set; } = new();
    public Dictionary<DiscordChannelId, List<DropperConfigurationRequest>> ChannelConfigurations { get; init; } = new();
}

public class DropperConfigurationRequest {
    public bool IsEnabled { get; set; }
    public List<DiscordUserId> WhiteListedUsers { get; set; } = new();
    public List<DiscordUserId> BlackListedUsers { get; set; }= new();
    public List<string> WhiteListedItems { get; init; }= new();
    public List<string> BlackListedItems { get; init; }= new();
    public List<string> WhiteListedSources { get; init; }= new();
    public List<string> BlackListedSources { get; init; }= new();

    public int MinimumGeValue { get; init; }
    public int MinimumHaValue { get; init; }
    public bool RequireBothGeAndHaValue { get; init; }

    public List<PlayerBuild> WhiteListedPlayerType { get; init; }= new();
    public List<PlayerBuild> BlackListedPlayerType { get; init; }= new();

    public bool AlwaysSendCollectionLogItems { get; init; }
}