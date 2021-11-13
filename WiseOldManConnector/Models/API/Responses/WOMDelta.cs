using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses; 

internal class WOMDelta {
    public double Start { get; set; }
    public double End { get; set; }

    [JsonProperty("gained")]
    public double Gained { get; set; }
}