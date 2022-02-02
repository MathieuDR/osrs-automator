using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output;

public abstract class MetricTypeLeaderboard<T> : Leaderboard<T> where T : ILeaderboardMember {
	protected MetricTypeLeaderboard() { }

	protected MetricTypeLeaderboard(MetricType metricType) {
		MetricType = metricType;
	}

	protected MetricTypeLeaderboard(List<T> items, MetricType metricType) : base(items) {
		MetricType = metricType;
	}

	/// <summary>
	///     From Request
	/// </summary>
	public MetricType MetricType { get; set; }
}