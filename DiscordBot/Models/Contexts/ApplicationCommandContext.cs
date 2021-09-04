using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Extensions;
using Discord.WebSocket;
using DiscordBot.Helpers.Extensions;

namespace DiscordBot.Models.Contexts {
    public class ApplicationCommandContext : BaseInteractiveContext<SocketSlashCommand> {
        public ApplicationCommandContext(SocketSlashCommand command, IServiceProvider provider) : base(command, provider) { }

        public NullValueDictionary<string, SocketSlashCommandDataOption> Options => InnerContext.Data.Options.ToNullValueDictionary();

        public NullValueDictionary<string, SocketSlashCommandDataOption> SubCommandOptions => Options.FirstOrDefault().Value?.Options.ToNullValueDictionary() ?? new NullValueDictionary<string, SocketSlashCommandDataOption>();

        public SocketSlashCommandDataOption GetOption(string name) {
            return InnerContext.Data.Options?.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
