﻿using System;
using Newtonsoft.Json;

namespace DiscordBotFanatic.Models.WiseOldMan.Responses.Models {
    public class GroupMember {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("lastImportedAt")]
        public DateTime? LastImportedAt { get; set; }

        [JsonProperty("registeredAt")]
        public DateTime RegisteredAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("overallExperience")]
        public int OverallExperience { get; set; }
    }
}