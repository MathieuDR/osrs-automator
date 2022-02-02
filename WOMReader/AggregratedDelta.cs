using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WOMReader;

public class AggregratedDelta {
	public int Id { get; set; }
	public string Name { get; set; }
	public Dictionary<MetricType, double> Gained { get; } = new();
	public DateTimeOffset Start { get; set; }
	public DateTimeOffset End { get; set; }
	public TimeSpan Duration => End - Start;
}