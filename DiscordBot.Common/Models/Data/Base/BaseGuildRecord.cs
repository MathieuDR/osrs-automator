namespace DiscordBot.Common.Models.Data.Base;

public record BaseGuildRecord : BaseRecord {
	public BaseGuildRecord() { }

	public BaseGuildRecord(ulong guildId, ulong discordId) {
		GuildId = guildId;
		CreatedById = discordId;
	}

	public ulong GuildId { get; init; }
	public ulong CreatedById { get; init; }
}