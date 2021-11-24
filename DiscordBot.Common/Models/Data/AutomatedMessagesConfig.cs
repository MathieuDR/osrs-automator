using DiscordBot.Common.Models.Enums;
using LiteDB;

namespace DiscordBot.Common.Models.Data;

public class AutomatedMessagesConfig {
    public Dictionary<JobType, ChannelJobConfiguration> ChannelJobs { get; set; } =
        new();
}

public class ChannelJobConfiguration {
    public ChannelJobConfiguration() { }

    public ChannelJobConfiguration(ulong guildId) : this(guildId, ulong.MinValue) { }

    public ChannelJobConfiguration(ulong guildId, ulong channelId) : this(guildId, channelId, false) { }

    public ChannelJobConfiguration(ulong guildId, ulong channelId, bool isEnabled) {
        GuildId = guildId;
        ChannelId = channelId;
        IsEnabled = isEnabled;
    }
    
    public bool IsEnabled { get; set; }
    public ulong ChannelId { get; set; }
    public ulong GuildId { get; set; }
}
