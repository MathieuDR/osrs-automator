using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Common.Models.Data.Configuration;

public class AutomatedMessagesConfig {
	public Dictionary<JobType, ChannelJobConfiguration> ChannelJobs { get; set; } =
		new();
}
