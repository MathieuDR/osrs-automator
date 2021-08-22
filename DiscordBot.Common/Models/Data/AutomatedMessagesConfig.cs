using System.Collections.Generic;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Common.Models.Data {
    public class AutomatedMessagesConfig {
        public Dictionary<JobType, ChannelJobConfiguration> ChannelJobs { get; set; } =
            new Dictionary<JobType, ChannelJobConfiguration>();
    }

    public class ChannelJobConfiguration {
        public ChannelJobConfiguration() { }

        public ChannelJobConfiguration(ulong guildId) : this(guildId, ulong.MinValue) { }

        public ChannelJobConfiguration(ulong guildId, ulong channelId) : this(guildId, channelId, false) { }

        public ChannelJobConfiguration(ulong guildId, ulong channelId, bool activated) {
            GuildId = guildId;
            ChannelId = channelId;
            Activated = activated;
        }

        public bool Activated { get; set; }
        public ulong ChannelId { get; set; }
        public ulong GuildId { get; set; }
    }
}