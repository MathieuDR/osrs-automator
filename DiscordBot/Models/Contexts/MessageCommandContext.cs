using System;
using Common.Extensions;
using Discord.WebSocket;

namespace DiscordBot.Models.Contexts; 

public class MessageCommandContext : BaseInteractiveContext<SocketMessageCommand>
{
    public MessageCommandContext(SocketMessageCommand innerContext, IServiceProvider provider)
        : base(innerContext, provider) { }

    public SocketUserMessage UserMessage => InnerContext.Data.Message.Cast<SocketUserMessage>();

    public override string Message => InnerContext.CommandName;
}