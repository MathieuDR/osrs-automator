using System.Text.Json.Serialization;

namespace DiscordBot.Dashboard.Models.Dink; 

public record Embed(
    [property: JsonPropertyName("image")] Image Image
);