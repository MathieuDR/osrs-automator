using System.Text.Json.Serialization;

namespace Dashboard.Models.ApiRequests.DiscordEmbed;

public class Embed {
    [JsonPropertyName("thumbnail")]
    public Thumbnail Thumbnail { get; set; }

    [JsonPropertyName("author")]
    public Author Author { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; }

    [JsonPropertyName("fields")]
    public List<Field> Fields { get; set; }
}
