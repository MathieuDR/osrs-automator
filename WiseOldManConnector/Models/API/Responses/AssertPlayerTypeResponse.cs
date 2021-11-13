using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses; 

internal class AssertPlayerTypeResponse : BaseResponse {
    [JsonProperty("type")]
    public string PlayerType { get; set; }
}