using System.Collections.Generic;
using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses; 

internal class GroupCreateResponse : WOMGroup {
    [JsonProperty("verificationCode")]
    public string VerificationCode { get; set; }

    [JsonProperty("members")]
    public List<PlayerResponse> Members { get; set; }
}