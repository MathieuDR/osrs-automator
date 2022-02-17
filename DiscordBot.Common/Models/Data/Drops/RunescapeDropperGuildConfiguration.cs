using DiscordBot.Common.Models.Data.Base;

namespace DiscordBot.Common.Models.Data.Drops;

public record RunescapeDropperGuildConfiguration : BaseGuildRecord {
    public RunescapeDropperGuildConfiguration() { }
    public RunescapeDropperGuildConfiguration(DiscordGuildId guildId, DiscordUserId userId) : base(guildId, userId) { }

    public IEnumerable<RunescapeDropperChannelConfiguration> EnabledChannels { get; init; }

    public IEnumerable<DiscordUserId> DisabledUsers { get; set; }
}