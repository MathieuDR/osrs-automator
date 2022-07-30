using DiscordBot.Common.Models.Data.Base;

namespace DiscordBot.Common.Models.Data.Graveyard;

public record Graveyard : BaseGuildRecord {
	public List<DiscordUserId> OptedInUsers { get; set; } = new();
	public Dictionary<DiscordUserId, List<Shame>> Shames { get; set; } = new();
}
