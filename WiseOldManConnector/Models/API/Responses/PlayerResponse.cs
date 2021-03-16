using System;
using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses {
    internal class PlayerResponse : BaseResponse {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("build")]
        public string Build { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("flagged")]
        public bool Flagged { get; set; }

        [JsonProperty("ehp")]
        public double EffectiveHoursPlayed { get; set; }

        [JsonProperty("ehb")]
        public double EffectiveHoursBossed { get; set; }

        [JsonProperty("ttm")]
        public double TimeToMax { get; set; }

        [JsonProperty("tt200m")]
        public double TimeTo200m { get; set; }

        [JsonProperty("lastImportedAt")]
        public DateTime? LastImportedAt { get; set; }

        [JsonProperty("registeredAt")]
        public DateTime? RegisteredAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

        [JsonProperty("latestSnapshot")]
        public WOMSnapshot LatestSnapshot { get; set; }

        [JsonProperty("combatLevel")]
        public int CombatLevel { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("exp")]
        public long OverallExperience { get; set; }
    }
}