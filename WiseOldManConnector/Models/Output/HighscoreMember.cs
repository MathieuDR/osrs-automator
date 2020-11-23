using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output {
    public class HighscoreMember{
        public Player Player { get; set; }
        public Metric Metric { get; set; }
        public MetricType MetricType { get; set; }
    }
}