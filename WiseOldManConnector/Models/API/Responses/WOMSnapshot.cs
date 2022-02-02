using Newtonsoft.Json;
using WiseOldManConnector.Deserializers;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Responses;

[JsonConverter(typeof(ObjectWithMetricsConvertor<WOMSnapshot, Metric>))]
internal class WOMSnapshot : BaseResponse, IMetricBearer<Metric> {
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("importedAt")]
    public object ImportedAt { get; set; }

    public Dictionary<MetricType, Metric> Metrics { get; set; } = new();
}
