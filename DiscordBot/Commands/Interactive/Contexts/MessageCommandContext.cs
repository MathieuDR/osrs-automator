using System;
using Common.Extensions;
using Discord.WebSocket;

namespace DiscordBot.Commands.Interactive.Contexts {
    public class MessageCommandContext : BaseInteractiveContext<SocketMessageCommand>
    {
        public MessageCommandContext(SocketMessageCommand backing, IServiceProvider provider)
            : base(backing, provider) { }

        public SocketUserMessage UserMessage => Backing.Data.Message.Cast<SocketUserMessage>();
    }
}