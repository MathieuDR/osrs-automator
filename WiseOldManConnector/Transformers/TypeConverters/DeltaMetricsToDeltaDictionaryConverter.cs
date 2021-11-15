using AutoMapper;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Transformers.TypeConverters;

internal class
    DeltaMetricsToDeltaDictionaryConverter : ITypeConverter<DeltaMetrics, Dictionary<MetricType, DeltaMetric>> {
    public Dictionary<MetricType, DeltaMetric> Convert(DeltaMetrics source,
        Dictionary<MetricType, DeltaMetric> destination, ResolutionContext context) {
        destination ??= new Dictionary<MetricType, DeltaMetric>();
        foreach (var kvp in source.Metrics) {
            destination.Add(kvp.Key, context.Mapper.Map<DeltaMetric>(kvp.Value));
        }

        // Set correct key!
        foreach (var kvp in destination) {
            kvp.Value.MetricType = kvp.Key;
        }

        return destination;
    }
}
