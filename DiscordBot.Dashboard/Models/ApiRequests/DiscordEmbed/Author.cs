using System.Text.Json.Serialization;

namespace DiscordBot.Dashboard.Models.ApiRequests.DiscordEmbed;

public class Author {
	[JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonPropertyName("url")]
	public string? Url { get; set; }

	[JsonPropertyName("icon_url")]
	public string? Icon { get; set; }
}
