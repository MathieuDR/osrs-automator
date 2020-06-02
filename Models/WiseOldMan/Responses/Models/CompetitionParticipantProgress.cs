using Newtonsoft.Json;

namespace DiscordBotFanatic.Models.WiseOldMan.Responses.Models {
    public class CompetitionParticipantProgress {
    

            [JsonProperty("start")]
            public int Start { get; set; }

            [JsonProperty("end")]
            public int End { get; set; }

            [JsonProperty("gained")]
            public int Gained { get; set; }
        
    }
}