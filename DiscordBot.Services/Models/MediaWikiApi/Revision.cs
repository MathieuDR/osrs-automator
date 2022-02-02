using System.Text.Json.Serialization;

namespace DiscordBot.Services.Models.MediaWikiApi;

public class Revision {
	[JsonPropertyName("contentformat")]
	public string Contentformat { get; set; }

	[JsonPropertyName("contentmodel")]
	public string Contentmodel { get; set; }

	[JsonPropertyName("*")]
	public string Content { get; set; }
}