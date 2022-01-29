using DiscordBot.Common.Models.Enums;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard.Helpers; 

public static class ShameExtensions {
	public static (ShameLocation shameLocation, MetricType? shameLocationMetric) ToLocation(string input, MetricTypeParser parser) {
		if (parser.TryParseToMetricType(input, out var metric)) {
			return (ShameLocation.MetricType, metric);
		}
		
		return ((ShameLocation)Enum.Parse(typeof(ShameLocation), input, true), null);
	}
}
