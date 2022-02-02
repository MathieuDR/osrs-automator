using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Responses;

internal interface IMetricBearer<T> where T : class, new() {
	Dictionary<MetricType, T> Metrics { get; }
}