using System.Collections.Generic;

namespace DiscordBotFanatic.Models.Data {
    public class GuildConfiguration :BaseModel{
        public ulong GuildId { get; set; }
        public List<GuildRole> GuildRoles { get; set; }
    }
}