using System.Text.Json.Serialization;

namespace DiscordBot.Dashboard.Models.Dink;

public record Item(
    [property: JsonPropertyName("id")] int? Id,
    [property: JsonPropertyName("quantity")]
    int? Quantity,
    [property: JsonPropertyName("priceEach")]
    int? PriceEach,
    [property: JsonPropertyName("name")] string Name
);