using System.Drawing;

namespace DiscordBot.Common.Models.DiscordDtos {
    public class Role {
        public string Name { get; set; }
        public ulong Id { get; set; }
        public Color Color { get; set; }
    }
}
