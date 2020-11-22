﻿using System.Collections.Generic;
using DiscordBotFanatic.Models.Enums;

namespace DiscordBotFanatic.Models.Data {
    public class AutomatedMessagesConfig {
        public AutomatedMessagesConfig() { }

        public Dictionary<JobTypes, ChannelJobConfiguration> ChannelJobs { get; set; } = new Dictionary<JobTypes, ChannelJobConfiguration>();

    }

    public class ChannelJobConfiguration {
        public ChannelJobConfiguration() {
            
        }

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