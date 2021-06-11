using System;
using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses {
    internal class CompetitionParticipantHistory {
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }
    }
}