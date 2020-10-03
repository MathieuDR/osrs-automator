using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses {
    internal class Participant {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("lastImportedAt")]
        public DateTime LastImportedAt { get; set; }

        [JsonProperty("registeredAt")]
        public DateTime RegisteredAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("history")]
        public List<CompetitionParticipantHistory> History { get; set; }

        [JsonProperty("progress")]
        public CompetitionParticipantProgress Progress { get; set; }
    }
}