using DiscordBot.Common.Configuration;

namespace DiscordBot.Configuration; 

public class BotConfiguration {
    public string Token { get; set; }

    public MessageConfiguration Messages { get; set; }
    public string CustomPrefix { get; set; }
}