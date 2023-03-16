using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses;

internal class LeaderboardMember : BaseResponse {
    [JsonProperty("player")]
    public PlayerResponse Player { get; set; }
    
    [JsonProperty("data")]
    public LeaderboardData Data { get; set; }
}


internal class LeaderboardData {
    [JsonProperty("rank")]
    public int? Rank { get; set; }
    [JsonProperty("level")]
    public int? Level { get; set; }
    [JsonProperty("experience")]
    public long? Experience { get; set; }
    [JsonProperty("kills")]
    public int? Kills { get; set; }
    [JsonProperty("Score")]
    public int? Score { get; set; }
    [JsonProperty("Value")]
    public long? Value { get; set; }
}
