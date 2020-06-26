using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses.Models {
    internal class CompetitionParticipantProgress {
    

            [JsonProperty("start")]
            public int Start { get; set; }

            [JsonProperty("end")]
            public int End { get; set; }

            [JsonProperty("gained")]
            public int Gained { get; set; }
        
    }
}