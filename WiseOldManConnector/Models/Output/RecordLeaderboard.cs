using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output;

public class RecordLeaderboard : MetricTypeAndPeriodLeaderboard<Record> {
	public RecordLeaderboard() { }
	public RecordLeaderboard(MetricType metricType, Period period) : base(metricType, period) { }
	public RecordLeaderboard(List<Record> items, MetricType metricType, Period period) : base(items, metricType, period) { }
}