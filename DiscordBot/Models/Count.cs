using System;
using Discord;

namespace DiscordBotFanatic.Models {
    public class Count {
        public int Additive { get; set; }
        public ulong RequestedBy { get; set; }
        public string RequestedDiscordTag { get; set; }
        public DateTimeOffset RequestedOn { get; set; }
        public string Reason { get; set; }

        public Count(IGuildUser requester, int additive , string reason) {
            Additive = additive;
            RequestedBy = requester.Id;
            RequestedDiscordTag = requester.Username;
            Reason = reason;
            RequestedOn = DateTimeOffset.Now;
        }

        public Count() { }
    }
}
