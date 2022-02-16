using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data.Base;

namespace DiscordBot.Common.Models.Data;

public record RunescapeDropperGuildConfiguration : BaseGuildRecord {
    public RunescapeDropperGuildConfiguration() { }
    public RunescapeDropperGuildConfiguration(ulong guildId, ulong discordId) : base(guildId, discordId) { }

    public IEnumerable<RunescapeDropperChannelConfiguration> EnabledChannels { get; init; }

    public IEnumerable<ulong> DisabledEndpoints { get; set; }
}