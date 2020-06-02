using System;
using Newtonsoft.Json;

namespace DiscordBotFanatic.Models.WiseOldMan.Responses.Models {
    public class CompetitionParticipantHistory {
        [JsonProperty("date")]
        public DateTime Date { get; set; }

        [JsonProperty("value")]
        public int Value { get; set; }
    }
}