using DiscordBot.Common.Models.Enums;
using LiteDB;

namespace DiscordBot.Common.Models.Data;

public class AutomatedMessagesConfig {
    public Dictionary<JobType, ChannelJobConfiguration> ChannelJobs { get; set; } =
        new();
}