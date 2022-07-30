using DiscordBot.Common.Identities;

namespace DiscordBot.Configuration;

public class BotTeamConfiguration {
	public DiscordGuildId GuildId { get; set; }
	public DiscordUserId OwnerId { get; set; }
}