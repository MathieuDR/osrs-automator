using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Common.Models.Data.Drops;

public record DropperConfiguration {
	public bool IsEnabled { get; init; }
	public List<DiscordUserId> WhiteListedUsers { get; init; }
	public List<DiscordUserId> BlackListedUsers { get; init; }
	public bool HasWhiteListedUsers => WhiteListedUsers.Any();
	public bool HasBlackListedUsers => BlackListedUsers.Any();
	public List<string> WhiteListedItems { get; init; }
	public List<string> BlackListedItems { get; init; }
	public bool HasWhiteListedItems => WhiteListedItems.Any();
	public bool HasBlackListedItems => BlackListedItems.Any();
	public List<string> WhiteListedSources { get; init; }
	public List<string> BlackListedSources { get; init; }
	public bool HasWhiteListedSources => WhiteListedSources.Any();
	public bool HasBlackListedSources => BlackListedSources.Any();

	public int MinimumGeValue { get; init; }
	public int MinimumHaValue { get; init; }
	public bool RequireBothGeAndHaValue { get; init; }

	public List<PlayerBuild> WhiteListedPlayerType { get; init; }
	public List<PlayerBuild> BlackListedPlayerType { get; init; }
	public bool HasWhiteListedPlayerType => WhiteListedPlayerType.Any();
	public bool HasBlackListedPlayerType => BlackListedPlayerType.Any();

	public bool AlwaysSendCollectionLogItems { get; init; }
}
