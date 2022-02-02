using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output;

public class CompetitionLeaderboard : MetricTypeLeaderboard<CompetitionParticipant> {
	public CompetitionLeaderboard() { }
	public CompetitionLeaderboard(MetricType metricType) : base(metricType) { }
	public CompetitionLeaderboard(List<CompetitionParticipant> items, MetricType metricType) : base(items, metricType) { }
}