using Newtonsoft.Json;
using WiseOldManConnector.Deserializers;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Responses;


internal class WOMSnapshot : BaseResponse {
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("importedAt")]
    public object ImportedAt { get; set; }

    public Dictionary<MetricType, Metric> Metrics => Data.Metrics;
    
    [JsonProperty("data")]
    public WOMMetrics Data { get; set; }
    
}


internal class WOMMetrics {
    [JsonProperty("skills")]
    public WOMMetricCategory Skills { get; set; }
    [JsonProperty("bosses")]
    public WOMMetricCategory Bosses { get; set; }
    [JsonProperty("activities")]
    public WOMMetricCategory Activities { get; set; }
    [JsonProperty("computed")]
    public WOMMetricCategory Computed { get; set; }

    public Dictionary<MetricType, Metric> Metrics => GetDictionary();

    private Dictionary<MetricType,Metric> GetDictionary() {
        var dictionaries = new Dictionary<MetricType, Metric>[] { Skills.Metrics, Bosses.Metrics, Activities.Metrics, Computed.Metrics };
        return dictionaries.SelectMany(dict => dict)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}

[JsonConverter(typeof(ObjectWithMetricsConvertor<WOMMetricCategory, Metric>))]
internal class WOMMetricCategory: IMetricBearer<Metric> {
    public Dictionary<MetricType, Metric> Metrics { get; } = new();
}
