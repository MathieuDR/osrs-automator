using Newtonsoft.Json;
using WiseOldManConnector.Deserializers;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Responses;

[JsonConverter(typeof(ObjectWithMetricsConvertor<DeltaMetrics, WOMDeltaMetric>))]
internal class DeltaMetrics : IMetricBearer<WOMDeltaMetric> {
    public Dictionary<MetricType, WOMDeltaMetric> Metrics { get; } = new();
}
