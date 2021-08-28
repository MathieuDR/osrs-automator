using System.Collections.Generic;

namespace DiscordBot.Common.Models.Data {
    public record RunescapeDropperGuildConfiguration : BaseGuildRecord {
        public RunescapeDropperGuildConfiguration() { }
        public RunescapeDropperGuildConfiguration(ulong guildId, ulong discordId) : base(guildId, discordId) { }

        public IEnumerable<RunescapeDropperChannelConfiguration> EnabledChannels { get; init; }
    }

    public record RunescapeDropperChannelConfiguration {
        public bool WhiteListEnabled { get; init; }
        public IEnumerable<string> WhiteListedItems { get; init; }
        public IEnumerable<string> BlackListedItems { get; init; }
        public bool UseCollectionLogExceptions { get; init; }
        public int MinimumHaValue { get; init; }
        public int MinimumValue { get; init; }
        public int MinRarity { get; init; }
        public bool OrOperator { get; init; }
        public ulong Channel { get; init; }
    }
}
