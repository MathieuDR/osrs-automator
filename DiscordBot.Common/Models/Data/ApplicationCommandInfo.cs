using DiscordBot.Common.Models.Data.Base;

namespace DiscordBot.Common.Models.Data;

public record ApplicationCommandInfo : BaseRecord {
    public ApplicationCommandInfo(string commandName) {
        CommandName = commandName;
    }

    public ApplicationCommandInfo() { }

    public string CommandName { get; init; }
    public uint Hash { get; init; }
    public bool IsGlobal { get; init; }
    public List<ulong> RegisteredGuilds { get; set; } = new();
}
