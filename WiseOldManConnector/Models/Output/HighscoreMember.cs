using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output;

public class HighscoreMember : ILeaderboardMember {
    public Metric Metric { get; set; }
    public MetricType MetricType { get; set; }
    public Player Player { get; set; }
    public double Value => Metric.Value;
}