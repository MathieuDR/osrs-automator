using System.Collections.Generic;
using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses {
    internal class GroupEditResponse : WOMGroup{
        [JsonProperty("members")]
        public List<PlayerResponse> Members { get; set; }
    }
}