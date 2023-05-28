using Newtonsoft.Json;
using WiseOldManConnector.Models.Output;

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

    [JsonProperty("memberships")]
    public WomGroupMember[] Members { get; set; }
}

internal sealed class WomGroupMember {
    [JsonProperty("player")]
    public PlayerResponse Player { get; set; }

    [JsonProperty("playerId")]
    public int Id { get; set; }

    [JsonProperty("groupId")]
    public int GroupId { get; set; }

    [JsonProperty("role")]
    public string Role { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt")]
    public DateTime UpdatedAt { get; set; }
}
