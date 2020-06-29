using System;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Responses.Models {
    internal class Record {
        public int Value { get; set; }
        public Period Period { get; set; }
        public MetricType Metric { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}