using System.Text.Json.Serialization;

namespace Dashboard.Models.ApiRequests.DiscordEmbed;

public class EmbedCollection {
    [JsonPropertyName("embeds")]
    public List<Embed> Embeds { get; set; }
}
