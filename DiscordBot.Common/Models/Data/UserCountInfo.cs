using System.Collections.Generic;
using System.Linq;
using DiscordBot.Common.Models.Data.Base;

namespace DiscordBot.Common.Models.Data {
    public class UserCountInfo : BaseModel {
        public UserCountInfo() { }
        public UserCountInfo(ulong userId) : base(userId) { }
        public ulong DiscordId { get; set; }

        public int CurrentCount => CountHistory.Sum(x => x.Additive);

        public List<Count> CountHistory { get; set; } = new();
    }
}
