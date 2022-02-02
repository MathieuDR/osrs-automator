using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output;

public class DeltaLeaderboard : MetricTypeAndPeriodLeaderboard<DeltaMember> {
	public DeltaLeaderboard() { }
	public DeltaLeaderboard(MetricType metricType, Period period) : base(metricType, period) { }

	public DeltaLeaderboard(List<DeltaMember> items, MetricType metricType, Period period) :
		base(items, metricType, period) { }
}