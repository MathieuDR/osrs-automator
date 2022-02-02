using System.Text.Json.Serialization;

namespace DiscordBot.Services.Models.MediaWikiApi;

public class Page {
	[JsonPropertyName("pageid")]
	public int Pageid { get; set; }

	[JsonPropertyName("ns")]
	public int Ns { get; set; }

	[JsonPropertyName("title")]
	public string Title { get; set; }

	[JsonPropertyName("revisions")]
	public List<Revision> Revisions { get; set; }
}