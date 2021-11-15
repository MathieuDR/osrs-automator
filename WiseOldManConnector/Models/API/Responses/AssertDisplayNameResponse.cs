using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses;

internal class AssertDisplayNameResponse : BaseResponse {
    [JsonProperty("displayName")]
    public string DisplayName { get; set; }
}
