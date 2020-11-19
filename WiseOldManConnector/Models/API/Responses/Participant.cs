using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses {
    internal class Participant : PlayerResponse {
        [JsonProperty("history")]
        public List<CompetitionParticipantHistory> History { get; set; }

        [JsonProperty("progress")]
        public CompetitionParticipantProgress Progress { get; set; }
    }
}