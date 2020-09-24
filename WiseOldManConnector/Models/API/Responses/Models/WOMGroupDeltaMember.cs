using System;
using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses.Models {
    internal class WOMGroupDeltaMember : BaseResponse {
        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("gained")]
        public int Gained { get; set; }

        [JsonProperty("player")]
        public PlayerResponse Player { get; set; }
    }
}