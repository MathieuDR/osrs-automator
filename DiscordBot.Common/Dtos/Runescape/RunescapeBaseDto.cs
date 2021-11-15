namespace DiscordBot.Common.Dtos.Runescape;

public abstract record RunescapeBaseDto {
    public string Name { get; init; }
    public string Url { get; init; }
}
