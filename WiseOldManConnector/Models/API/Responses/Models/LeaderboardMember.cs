using System;
using Newtonsoft.Json;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Responses.Models {
    internal class LeaderboardMember : BaseResponse {
        [JsonProperty("playerId")]
        public int PlayerId { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("type")]
        public string PlayerType { get; set; }

        [JsonProperty("startDate")]
        public DateTime StartDate { get; set; }

        [JsonProperty("endDate")]
        public DateTime EndDate { get; set; }

        [JsonProperty("registeredAt")]
        public DateTime RegisteredAt { get; set; }

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

        [JsonProperty("level")]
        public int Level { get; set; }
    }
}