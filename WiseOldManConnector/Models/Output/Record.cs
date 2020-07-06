using System;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output {
    public class Record {
        public int Value { get; set; }
        public MetricType MetricType { get; set; }
        public Period Period { get; set; }
        public DateTimeOffset UpdateDateTime { get; set; }
        public PlayerType PlayerType { get; set; }
        public string Username { get; set; }
    }
}