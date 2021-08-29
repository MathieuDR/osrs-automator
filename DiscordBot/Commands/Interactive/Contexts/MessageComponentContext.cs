using System;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;

namespace DiscordBot.Commands.Interactive.Contexts {
    public class MessageComponentContext : BaseInteractiveContext<SocketMessageComponent>
    {
        public MessageComponentContext(SocketMessageComponent backing, IServiceProvider provider)
            : base(backing, provider) { }

        public string CustomId => Backing.Data.CustomId;
        public string[] CustomIdParts => CustomId.Split(':');

        public IEnumerable<string> SelectedMenuOptions
            => Backing.Data.Values?.ToHashSet() ?? new HashSet<string>();
    }
}