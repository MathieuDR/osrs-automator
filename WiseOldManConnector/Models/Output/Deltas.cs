using System;
using System.Collections.Generic;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output; 

public class Deltas : IBaseConnectorOutput {
    public Period Period { get; set; }
    public DateTimeOffset StartDateTime { get; set; }
    public DateTimeOffset EndDateTime { get; set; }

    public Dictionary<MetricType, DeltaMetric> DeltaMetrics { get; set; }
}