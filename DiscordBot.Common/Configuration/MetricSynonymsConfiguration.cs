using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Common.Configuration;

public class MetricSynonymsConfiguration {
	public Dictionary<MetricType, List<string>> Synonyms { get; set; }
}
