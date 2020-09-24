using System;
using Newtonsoft.Json;
using WiseOldManConnector.Models.API.Responses.Models;

namespace WiseOldManConnector.Models.API.Responses {
    internal class PlayerResponse : BaseResponse {

        [JsonProperty("build")]
        public string Build { get; set; }

        [JsonProperty("flagged")]
        public bool Flagged { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("lastImportedAt")]
        public DateTime? LastImportedAt { get; set; }

        [JsonProperty("registeredAt")]
        public DateTime? RegisteredAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("latestSnapshot")]
        public WOMSnapshot LatestSnapshot { get; set; }

        [JsonProperty("combatLevel")]
        public int CombatLevel { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("overallExperience")]
        public int OverallExperience { get; set; }
    }
}