using System;
using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;

namespace DiscordBot.Commands.Interactive.Contexts {
    public class ApplicationCommandContext : BaseInteractiveContext<SocketSlashCommand>
    {
        public ApplicationCommandContext(SocketSlashCommand command, IServiceProvider provider) : base(command, provider) { }

        public Dictionary<string, object> ValuedOptions
            => Backing.Data.Options.ToDictionary(x => x.Name, x => x.Value);

        public Dictionary<string, SocketSlashCommandDataOption> Options
            => Backing.Data.Options?.ToDictionary(x => x.Name);

        public SocketSlashCommandDataOption GetOption(string name) =>
            Backing.Data.Options?.FirstOrDefault(x => String.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase));
    }
}