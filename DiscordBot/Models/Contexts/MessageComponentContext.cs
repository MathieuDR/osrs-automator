using System;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;

namespace DiscordBot.Models.Contexts {
    public class MessageComponentContext : BaseInteractiveContext<SocketMessageComponent>
    {
        public MessageComponentContext(SocketMessageComponent innerContext, IServiceProvider provider)
            : base(innerContext, provider) { }

        public string CustomId => InnerContext.Data.CustomId;
        public string[] CustomIdParts => CustomId.Split(':');

        public IEnumerable<string> SelectedMenuOptions
            => InnerContext.Data.Values?.ToHashSet() ?? new HashSet<string>();
    }
}