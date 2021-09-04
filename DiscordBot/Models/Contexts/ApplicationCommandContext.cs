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

        /// <summary>
        ///     Retrieves an value of an option from the slash command options.
        ///     Use <see cref="GetOptionValue{T}" /> to get the value with a default of a requested type (Eg Booleans)
        /// </summary>
        public NullValueDictionary<string, object> ValueOptions =>
            new(InnerContext.Data.Options?.ToDictionary(x => x.Name, x => x.Value) ?? new Dictionary<string, object>());

        public NullValueDictionary<string, SocketSlashCommandDataOption> Options => InnerContext.Data.Options.ToNullValueDictionary();

        public SocketSlashCommandDataOption GetOption(string name) {
            return InnerContext.Data.Options?.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
