using Common.Extensions;

namespace DiscordBot.Models.Contexts; 

public class UserCommandContext : BaseInteractiveContext<SocketUserCommand>
{
    public UserCommandContext(SocketUserCommand innerContext, IServiceProvider provider)
        : base(innerContext, provider) { }

    public SocketGuildUser TargetedGuildUser => InnerContext.Data.Member.Cast<SocketGuildUser>();
        
    public override string Message => InnerContext.CommandName;
}