using System.Text.Json.Serialization;

namespace DiscordBot.Dashboard.Models.Dink;

public record Content(
    [property: JsonPropertyName("items")] IReadOnlyList<Item> Items,
    [property: JsonPropertyName("source")] string Source,
    [property: JsonPropertyName("category")]
    string Category
);