using Common;

namespace DiscordBot.Helpers.Extensions; 

public static class CommandContextExtensions {
    public static SocketSlashCommandDataOption GetOption(this DefaultDictionary<string, SocketSlashCommandDataOption> valueDictionary, string optionName) {
        return valueDictionary[optionName];
    }
    public static object GetOptionValue(this DefaultDictionary<string, SocketSlashCommandDataOption> valueDictionary, string optionName) {
        return valueDictionary.GetOption(optionName)?.Value;
    }
        
    public static T GetOptionValue<T>(this DefaultDictionary<string, SocketSlashCommandDataOption> valueDictionary, string optionName) {
        return (T)(valueDictionary.GetOptionValue(optionName) ?? default(T));
    }
        
    public static DefaultDictionary<string, T> ToDefaultDictionary<T>(this IReadOnlyCollection<T> options) where T:IApplicationCommandInteractionDataOption{
        return new(options?.ToDictionary(x => x.Name) ?? new Dictionary<string, T>());
    }
}