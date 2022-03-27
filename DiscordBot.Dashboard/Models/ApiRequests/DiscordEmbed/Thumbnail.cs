using System.Text.Json.Serialization;

namespace DiscordBot.Dashboard.Models.ApiRequests.DiscordEmbed;

public class Thumbnail {
	[JsonPropertyName("url")]
	public string Url { get; set; }
}
