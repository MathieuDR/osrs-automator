namespace DiscordBot.Common.Models.Data.Configuration;

public class ChannelJobConfiguration {
	public ChannelJobConfiguration() { }

	public ChannelJobConfiguration(DiscordGuildId guildId) : this(guildId, DiscordChannelId.Empty) { }

	public ChannelJobConfiguration(DiscordGuildId guildId, DiscordChannelId channelId) : this(guildId, channelId, false) { }

	public ChannelJobConfiguration(DiscordGuildId guildId, DiscordChannelId channelId, bool isEnabled) {
		GuildId = guildId;
		ChannelId = channelId;
		IsEnabled = isEnabled;
	}

	public bool IsEnabled { get; set; }
	public DiscordChannelId ChannelId { get; set; }
	public DiscordGuildId GuildId { get; set; }
}
