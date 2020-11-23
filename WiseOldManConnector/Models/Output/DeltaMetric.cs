using System.Collections.Generic;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output {
    public class DeltaMetric{
        public MetricType MetricType { get; set; }

        public Dictionary<DeltaType, Delta> Deltas { get; set; }
    }
}