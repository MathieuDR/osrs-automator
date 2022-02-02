using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output;

public class HighscoreLeaderboard : MetricTypeLeaderboard<HighscoreMember> {
	public HighscoreLeaderboard() { }
	public HighscoreLeaderboard(MetricType metricType) : base(metricType) { }
	public HighscoreLeaderboard(List<HighscoreMember> items, MetricType metricType) : base(items, metricType) { }
}