using System;
using Newtonsoft.Json;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Responses.Models {
    public class LeaderboardMember {
        [JsonProperty("playerId")]
        public int PlayerId { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("type")]
        public PlayerType PlayerType { get; set; }

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
    }
}