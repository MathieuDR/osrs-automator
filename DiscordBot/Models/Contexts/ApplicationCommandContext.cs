using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Discord.WebSocket;

namespace DiscordBot.Models.Contexts {
    public class ApplicationCommandContext : BaseInteractiveContext<SocketSlashCommand>
    {
        public ApplicationCommandContext(SocketSlashCommand command, IServiceProvider provider) : base(command, provider) { }

        public NullValueDictionary<string, object> ValueOptions
            =>
                new(InnerContext.Data.Options?.ToDictionary(x => x.Name, x => x.Value) ?? new Dictionary<string, object>());

        public NullValueDictionary<string, SocketSlashCommandDataOption> Options
            => new(InnerContext.Data.Options?.ToDictionary(x => x.Name) ?? new Dictionary<string, SocketSlashCommandDataOption>());

        public SocketSlashCommandDataOption GetOption(string name) =>
            InnerContext.Data.Options?.FirstOrDefault(x => String.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase));
    }
}