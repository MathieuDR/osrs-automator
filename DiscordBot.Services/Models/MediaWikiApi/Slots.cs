using System.Text.Json.Serialization;

namespace DiscordBot.Services.Models.MediaWikiApi;

public class Slots {
    [JsonPropertyName("main")]
    public Main Main { get; set; }
}
