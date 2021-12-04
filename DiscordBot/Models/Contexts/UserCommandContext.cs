using Common.Extensions;

namespace DiscordBot.Models.Contexts;

public class UserCommandContext : BaseInteractiveContext<SocketUserCommand> {
    public UserCommandContext(SocketUserCommand innerContext, IServiceProvider provider)
        : base(innerContext, provider) { }

    public SocketGuildUser TargetedGuildUser => InnerContext.Data.Member.Cast<SocketGuildUser>();
    public override string Message => Command;
    public override string Command => InnerContext.CommandName;
    public override string SubCommand => InnerContext.Data.Options.Where(x => x.Type == ApplicationCommandOptionType.SubCommand).Select(x => x.Name)
        .FirstOrDefault();

    public override string SubCommandGroup => InnerContext.Data.Options.Where(x => x.Type == ApplicationCommandOptionType.SubCommandGroup).Select(x => x.Name)
        .FirstOrDefault();
}
