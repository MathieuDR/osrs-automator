namespace DiscordBot.Common.Dtos.Discord;

public class Guild : GuildEntity {
    public string Name { get; set; }
    public DiscordGuildId Id => GuildId;
}
