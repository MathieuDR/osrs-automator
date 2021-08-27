using System;
using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses {
    internal class LeaderboardMember : BaseResponse {
        [JsonProperty("player")]
        public PlayerResponse Player { get; set; }

        [JsonProperty("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonProperty("endValue")]
        public int? EndValue { get; set; }

        [JsonProperty("startValue")]
        public int? StartValue { get; set; }

        [JsonProperty("gained")]
        public int? Gained { get; set; }

        [JsonProperty("rank")]
        public int? Rank { get; set; }

        [JsonProperty("experience")]
        public int? Experience { get; set; }

        [JsonProperty("value")]
        public int? Value { get; set; }

        [JsonProperty("level")]
        public int? Level { get; set; }

        [JsonProperty("kills")]
        public int? Kills { get; set; }

        [JsonProperty("score")]
        public int? Score { get; set; }
    }
}
