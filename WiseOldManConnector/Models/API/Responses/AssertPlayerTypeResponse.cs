using Newtonsoft.Json;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Responses {
    internal class AssertPlayerTypeResponse : BaseResponse {
        [JsonProperty("type")]
        public string PlayerType { get; set; }
    }
}