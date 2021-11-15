using System.Drawing;

namespace DiscordBot.Common.Dtos.Discord;

public class Role : GuildEntity {
    public string Name { get; set; }
    public ulong Id { get; set; }
    public Color Color { get; set; }
}
