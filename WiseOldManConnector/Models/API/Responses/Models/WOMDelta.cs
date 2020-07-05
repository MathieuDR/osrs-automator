using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses.Models {
    internal class WOMDelta {
        public int Start { get; set; }
        public int End { get; set; }

        [JsonProperty("gained")]
        public int Gained { get; set; }
    }
}