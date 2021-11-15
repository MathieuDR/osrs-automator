using Newtonsoft.Json;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Requests;

public class MemberRequest {
    [JsonProperty("username")]
    public string Name { get; set; }

    [JsonProperty("role")]
    public GroupRole Role { get; set; }
}
