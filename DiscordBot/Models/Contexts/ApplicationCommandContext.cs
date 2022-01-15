using Common;

namespace DiscordBot.Models.Contexts;

public class ApplicationCommandContext : BaseInteractiveContext<SocketSlashCommand> {
    public ApplicationCommandContext(SocketSlashCommand command, IServiceProvider provider) : base(command, provider) { }

    public DefaultDictionary<string, SocketSlashCommandDataOption> Options => InnerContext.Data.Options.ToDefaultDictionary();

    public DefaultDictionary<string, SocketSlashCommandDataOption> SubCommandOptions =>
        Options.FirstOrDefault().Value?.Options.ToDefaultDictionary() ?? new DefaultDictionary<string, SocketSlashCommandDataOption>();
    
    public SocketSlashCommandDataOption GetOption(string name) {
        return InnerContext.Data.Options?.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase));
    }
    
    public override string Message => Command;
    public override string Command => InnerContext.CommandName;
    public override string SubCommand => InnerContext.Data.Options.Where(x => x.Type == ApplicationCommandOptionType.SubCommand).Select(x => x.Name)
        .FirstOrDefault();

    public override string SubCommandGroup => InnerContext.Data.Options.Where(x => x.Type == ApplicationCommandOptionType.SubCommandGroup).Select(x => x.Name)
        .FirstOrDefault();
}
