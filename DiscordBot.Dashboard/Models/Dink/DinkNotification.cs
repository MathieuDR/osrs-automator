using System.Text.Json.Serialization;

namespace DiscordBot.Dashboard.Models.Dink;

public record DinkNotification(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("playerName")]
    string PlayerName,
    [property: JsonPropertyName("accountType")]
    string AccountType,
    [property: JsonPropertyName("extra")] Content Content,
    [property: JsonPropertyName("content")]
    string Message,
    [property: JsonPropertyName("embeds")] IReadOnlyList<Embed> Embeds
);