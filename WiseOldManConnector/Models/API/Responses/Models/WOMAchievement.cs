using System;
using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses.Models {
    internal class WOMAchievement : BaseResponse {
        [JsonProperty("threshold")]
        public long Threshold { get; set; }

        [JsonProperty("playerId")]
        public int PlayerId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("metric")]
        public string Metric { get; set; }

        [JsonProperty("createdAt")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("missing")]
        public Boolean Missing { get; set; }
    }
}