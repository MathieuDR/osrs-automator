using System;
using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses; 

internal class WOMAchievement : BaseResponse {
    [JsonProperty("threshold")]
    public long Threshold { get; set; }

    [JsonProperty("playerId")]
    public int PlayerId { get; set; }

    [JsonProperty("name")]
    public string Type { get; set; }

    [JsonProperty("metric")]
    public string Metric { get; set; }

    [JsonProperty("createdAt")]
    public DateTime? CreatedAt { get; set; }

    [JsonProperty("missing")]
    public bool Missing { get; set; }

    [JsonProperty("player")]
    public PlayerResponse Player { get; set; }
}