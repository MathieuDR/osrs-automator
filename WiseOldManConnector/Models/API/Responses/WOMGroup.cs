using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses;

internal class WOMGroup : BaseResponse {
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("score")]
    public int Score { get; set; }

    [JsonProperty("memberCount")]
    public int MemberCount { get; set; }

    [JsonProperty("clanChat")]
    public string ClanChat { get; set; }

    [JsonProperty("verified")]
    public bool Verified { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
