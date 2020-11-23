using System.Collections.Generic;
using System.Linq;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output {
    public class Snapshot : IBaseConnectorOutput{
        public Dictionary<MetricType, Metric> AllMetrics { get; set; } = new Dictionary<MetricType, Metric>();

        public Metric GetMetricForType(MetricType metric) {
            return !AllMetrics.ContainsKey(metric) ? null : AllMetrics[metric];
        }

        public Dictionary<MetricType, Metric> GetMetricTypeCategory(MetricTypeCategory category) {
            var validMetrics = category.GetMetricTypes();
            return AllMetrics.Where(x => validMetrics.Contains(x.Key)).ToDictionary(x=>x.Key, x=>x.Value);
        }
    }
}