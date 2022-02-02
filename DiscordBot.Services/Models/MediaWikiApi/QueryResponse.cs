using System.Text.Json.Serialization;

namespace DiscordBot.Services.Models.MediaWikiApi;

public class QueryResponse {
    [JsonPropertyName("batchcomplete")]
    public string Batchcomplete { get; set; }

    [JsonPropertyName("query")]
    public Query Query { get; set; }
}
