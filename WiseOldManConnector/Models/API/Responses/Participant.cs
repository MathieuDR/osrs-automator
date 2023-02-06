using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses;

internal class Participant : BaseResponse {
    [JsonProperty("progress")]
    public CompetitionParticipantProgress Progress { get; set; }

    [JsonProperty("player")]
    public PlayerResponse Player { get; set; }
}
