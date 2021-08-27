using System.Text.Json.Serialization;

namespace Dashboard.Models.ApiRequests.DiscordEmbed {
    public class Thumbnail {
        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
