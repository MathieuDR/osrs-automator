using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses;

internal class NameChangeResponse : BaseResponse {
    [JsonProperty("id")]
    public int Id { get; set; }

    [JsonProperty("playerId")]
    public int PlayerId { get; set; }

    [JsonProperty("oldName")]
    public string OldName { get; set; }

    [JsonProperty("newName")]
    public string NewName { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }

    [JsonProperty("resolvedAt")]
    public DateTime? ResolvedAt { get; set; }

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updatedAt")]
    public DateTime? UpdatedAt { get; set; }
}
