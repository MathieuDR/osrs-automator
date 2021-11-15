using DiscordBot.Common.Models.Data;

namespace DiscordBot.Common.Dtos.Discord; 

public class Guild : GuildConfig{
    public string Name { get; set; }
    public ulong Id => GuildId;
}