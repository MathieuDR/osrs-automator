using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses;

internal class DeltaFullResponse : BaseResponse {
    // TODO Rename
    public DeltaResponse Day { get; set; }
    public DeltaResponse Year { get; set; }
    public DeltaResponse Month { get; set; }
    public DeltaResponse Week { get; set; }
}

internal class DeltaResponse : BaseResponse {
    // TODO Rename
    public string Period { get; set; }

    //public DateTime UpdatedAt { get; set; }
    public DateTime StartsAt { get; set; }

    public DateTime EndsAt { get; set; }
    //public string Interval { get; set; }

    [JsonProperty("data")]
    public DeltaMetrics Metrics { get; set; }
}
