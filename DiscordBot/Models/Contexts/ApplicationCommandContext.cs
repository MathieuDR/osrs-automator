using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Discord.WebSocket;

namespace DiscordBot.Models.Contexts {
    public class ApplicationCommandContext : BaseInteractiveContext<SocketSlashCommand> {
        public ApplicationCommandContext(SocketSlashCommand command, IServiceProvider provider) : base(command, provider) { }

        /// <summary>
        ///     Retrieves an value of an option from the slash command options.
        ///     Use <see cref="GetOptionValue{T}" /> to get the value with a default of a requested type (Eg Booleans)
        /// </summary>
        public NullValueDictionary<string, object> ValueOptions =>
            new(InnerContext.Data.Options?.ToDictionary(x => x.Name, x => x.Value) ?? new Dictionary<string, object>());

        public NullValueDictionary<string, SocketSlashCommandDataOption> Options => new(InnerContext.Data.Options?.ToDictionary(x => x.Name) ??
                                                                                        new Dictionary<string, SocketSlashCommandDataOption>());

        public SocketSlashCommandDataOption GetOption(string name) {
            return InnerContext.Data.Options?.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        ///     Retrieves an value of an option from the slash command options.
        ///     Use <see cref="ValueOptions" /> to determine if the option is set.
        /// </summary>
        /// <param name="key">Key of the option</param>
        /// <typeparam name="T">Return type</typeparam>
        /// <returns>Value or default if not set</returns>
        public T GetOptionValue<T>(string key) {
            return (T) (ValueOptions[key] ?? default(T));
        }
    }
}
