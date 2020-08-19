using Newtonsoft.Json;
using WiseOldManConnector.Models.API.Responses.Models;

namespace WiseOldManConnector.Models.API.Responses {
    internal class StatisticsResponse : BaseResponse {
        [JsonProperty("maxedCombatCount")]
        public int MaxedCombatCount { get; set; }

        [JsonProperty("maxedTotalCount")]
        public int MaxedTotalCount { get; set; }

        [JsonProperty("maxed200msCount")]
        public int Maxed200msCount { get; set; }

        [JsonProperty("averageStats")]
        public WOMSnapshot AverageStats { get; set; }
    }
}