using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses.Models {
    internal class Metric {
        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("kills")]
        public int Kills { get; set; }
    }
}