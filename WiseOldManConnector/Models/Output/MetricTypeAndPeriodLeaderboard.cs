using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output;

public abstract class MetricTypeAndPeriodLeaderboard<T> : MetricTypeLeaderboard<T> where T : ILeaderboardMember {
	protected MetricTypeAndPeriodLeaderboard() { }


	protected MetricTypeAndPeriodLeaderboard(MetricType metricType, Period period) : base(metricType) {
		Period = period;
	}

	protected MetricTypeAndPeriodLeaderboard(List<T> items, MetricType metricType, Period period) : base(items, metricType) {
		Period = period;
	}

	/// <summary>
	///     From Request
	/// </summary>
	public Period Period { get; set; }
}