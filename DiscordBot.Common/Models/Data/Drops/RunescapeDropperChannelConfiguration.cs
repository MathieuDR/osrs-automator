namespace DiscordBot.Common.Models.Data.Drops;

public record RunescapeDropperChannelConfiguration {
	public bool WhiteListEnabled { get; init; }
	public IEnumerable<string> WhiteListedItems { get; init; }
	public IEnumerable<string> BlackListedItems { get; init; }
	public bool UseCollectionLogExceptions { get; init; }
	public int MinimumHaValue { get; init; }
	public int MinimumValue { get; init; }
	public int MinRarity { get; init; }
	public bool OrOperator { get; init; }
	public DiscordChannelId Channel { get; init; }
}