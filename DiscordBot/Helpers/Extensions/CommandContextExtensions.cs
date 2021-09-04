using System.Collections.Generic;
using System.Linq;
using Common;
using Discord;
using Discord.WebSocket;

namespace DiscordBot.Helpers.Extensions {
    public static class CommandContextExtensions {
        public static SocketSlashCommandDataOption GetOption(this NullValueDictionary<string, SocketSlashCommandDataOption> valueDictionary, string optionName) {
            return valueDictionary[optionName];
        }
        public static object GetOptionValue(this NullValueDictionary<string, SocketSlashCommandDataOption> valueDictionary, string optionName) {
            return valueDictionary.GetOption(optionName)?.Value;
        }
        
        public static T GetOptionValue<T>(this NullValueDictionary<string, SocketSlashCommandDataOption> valueDictionary, string optionName) {
            return (T)(valueDictionary.GetOptionValue(optionName) ?? default(T));
        }
        
        public static NullValueDictionary<string, T> ToNullValueDictionary<T>(this IReadOnlyCollection<T> options) where T:IApplicationCommandInteractionDataOption{
            return new(options?.ToDictionary(x => x.Name) ?? new Dictionary<string, T>());
        }
    }
}
