using System.Text.Json.Serialization;

namespace DiscordBot.Services.Models.MediaWikiApi;

public class Query {
	[JsonPropertyName("pages")]
	public Dictionary<string, Page> Pages { get; set; }
}