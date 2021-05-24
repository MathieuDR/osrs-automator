using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses {
    internal class WOMDelta {
        public int Start { get; set; }
        public int End { get; set; }

        [JsonProperty("gained")]
        public double Gained { get; set; }
    }
}