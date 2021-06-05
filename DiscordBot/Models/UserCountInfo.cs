using System.Collections.Generic;
using System.Linq;
using Discord;
using DiscordBotFanatic.Models.Data;
using DiscordBotFanatic.Modules;

namespace DiscordBotFanatic.Models {
    public class UserCountInfo : BaseModel {
        public UserCountInfo() { }
        public UserCountInfo(IUser user) : base(user) { }
        public UserCountInfo(ulong userId) : base(userId) { }
        public ulong DiscordId { get; set; }

        public int CurrentCount => CountHistory.Sum(x => x.Additive);
        
        public List<Count> CountHistory { get; set; } = new List<Count>();
    }
}
