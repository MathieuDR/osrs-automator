using System;
using Newtonsoft.Json;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Responses.Models {
    internal class LeaderboardMember : PlayerResponse {
        [JsonProperty("playerId")]
        private int PlayerId { set => Id = value; }

        [JsonProperty("type")]
        public string PlayerType { get; set; }

        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("endValue")]
        public int EndValue { get; set; }

        [JsonProperty("startValue")]
        public int StartValue { get; set; }

        [JsonProperty("gained")]
        public int Gained { get; set; }

        [JsonProperty("rank")]
        public int Rank { get; set; }

        [JsonProperty("experience")]
        public int Experience { get; set; }

        [JsonProperty("value")]
        public int Value{ get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }
    }
}