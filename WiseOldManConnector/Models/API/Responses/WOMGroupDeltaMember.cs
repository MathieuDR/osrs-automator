using System;
using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses {
    internal class WOMGroupDeltaMember : BaseResponse {
        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("gained")]
        public double Gained { get; set; }

        [JsonProperty("player")]
        public PlayerResponse Player { get; set; }
    }
}
