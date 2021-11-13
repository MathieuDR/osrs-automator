namespace DiscordBot.Common.Dtos.Runescape; 

public record RunescapeItem : RunescapeBaseDto {
    public int Value { get; init; }
    public int HaValue { get; init; }
    public string Thumbnail { get; init; }
}