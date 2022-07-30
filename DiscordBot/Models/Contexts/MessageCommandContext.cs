using MathieuDR.Common.Extensions;

namespace DiscordBot.Models.Contexts;

public class MessageCommandContext : BaseInteractiveContext<SocketMessageCommand> {
    public MessageCommandContext(SocketMessageCommand innerContext, IServiceProvider provider)
        : base(innerContext, provider) { }

    public SocketUserMessage UserMessage => InnerContext.Data.Message.Cast<SocketUserMessage>();
    
    public override string Message => Command;
    public override string Command => InnerContext.CommandName;
    public override string SubCommand => InnerContext.Data.Options.Where(x => x.Type == ApplicationCommandOptionType.SubCommand).Select(x => x.Name)
        .FirstOrDefault();

    public override string SubCommandGroup => InnerContext.Data.Options.Where(x => x.Type == ApplicationCommandOptionType.SubCommandGroup).Select(x => x.Name)
        .FirstOrDefault();
}
