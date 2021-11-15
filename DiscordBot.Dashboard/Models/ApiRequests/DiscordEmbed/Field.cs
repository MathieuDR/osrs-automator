using System.Text.Json.Serialization;

namespace Dashboard.Models.ApiRequests.DiscordEmbed;

public class Field {
    [JsonPropertyName("inline")]
    public bool Inline { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; }
}
