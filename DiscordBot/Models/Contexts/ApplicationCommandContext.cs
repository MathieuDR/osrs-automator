using System;
using System.Linq;
using Common;
using Discord.WebSocket;
using DiscordBot.Helpers.Extensions;

namespace DiscordBot.Models.Contexts; 

public class ApplicationCommandContext : BaseInteractiveContext<SocketSlashCommand> {
    public ApplicationCommandContext(SocketSlashCommand command, IServiceProvider provider) : base(command, provider) { }

    public DefaultDictionary<string, SocketSlashCommandDataOption> Options => InnerContext.Data.Options.ToDefaultDictionary();

    public DefaultDictionary<string, SocketSlashCommandDataOption> SubCommandOptions =>
        Options.FirstOrDefault().Value?.Options.ToDefaultDictionary() ?? new DefaultDictionary<string, SocketSlashCommandDataOption>();

    public SocketSlashCommandDataOption GetOption(string name) {
        return InnerContext.Data.Options?.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase));
    }
        
    public override string Message => InnerContext.CommandName;
}