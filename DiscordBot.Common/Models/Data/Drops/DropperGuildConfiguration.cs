using DiscordBot.Common.Models.Data.Base;

namespace DiscordBot.Common.Models.Data.Drops;

public record DropperGuildConfiguration : BaseGuildRecord {
	public DropperGuildConfiguration() { }
	public DropperGuildConfiguration(DiscordGuildId guildId, DiscordUserId userId) : base(guildId, userId) { }

	public bool IsEnabled { get; init; }
	public IEnumerable<DiscordUserId> DisabledUsers { get; init; } = new List<DiscordUserId>();
	public DropperConfiguration MasterConfiguration { get; init; } = new();
	public Dictionary<DiscordChannelId, List<DropperConfiguration>> ChannelConfigurations { get; init; } = new();
	public Dictionary<DiscordUserId, EndpointId> UserEndpoints { get; set; } = new();
}