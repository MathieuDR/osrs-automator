using System;
using Common.Extensions;
using Discord.WebSocket;

namespace DiscordBot.Commands.Interactive.Contexts {
    public class MessageCommandContext : BaseInteractiveContext<SocketMessageCommand>
    {
        public MessageCommandContext(SocketMessageCommand innerContext, IServiceProvider provider)
            : base(innerContext, provider) { }

        public SocketUserMessage UserMessage => InnerContext.Data.Message.Cast<SocketUserMessage>();
    }
}