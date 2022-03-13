namespace DiscordBot.Common.Models.Data.Base;

public record BaseGuildRecord : BaseRecord {
	public BaseGuildRecord() { }

	public BaseGuildRecord(DiscordGuildId guildId, DiscordUserId userId) {
		GuildId = guildId;
		CreatedById = userId;
	}

	public DiscordGuildId GuildId { get; init; }
	public DiscordUserId CreatedById { get; init; }
}
