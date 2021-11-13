using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses; 

internal class CompetitionParticipantProgress {
    [JsonProperty("start")]
    public double Start { get; set; }

    [JsonProperty("end")]
    public double End { get; set; }

    [JsonProperty("gained")]
    public double Gained { get; set; }
}