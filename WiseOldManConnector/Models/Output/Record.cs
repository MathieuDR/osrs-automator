using System;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output {
    public class Record : IBaseConnectorOutput, ILeaderboardMember {
        public double Value { get; set; }
        public MetricType MetricType { get; set; }
        public Period Period { get; set; }
        public DateTimeOffset UpdateDateTime { get; set; }
        public Player Player { get; set; }
    }
}
