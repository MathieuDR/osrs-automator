using System;
using Common.Extensions;
using Discord.WebSocket;

namespace DiscordBot.Commands.Interactive.Contexts {
    public class UserCommandContext : BaseInteractiveContext<SocketUserCommand>
    {
        public UserCommandContext(SocketUserCommand backing, IServiceProvider provider)
            : base(backing, provider) { }

        public SocketGuildUser TargetedGuildUser => Backing.Data.Member.Cast<SocketGuildUser>();
    }
}