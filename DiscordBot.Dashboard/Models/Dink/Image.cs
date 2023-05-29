using System.Text.Json.Serialization;

namespace DiscordBot.Dashboard.Models.Dink;

public record Image(
    [property: JsonPropertyName("url")] string Url
);