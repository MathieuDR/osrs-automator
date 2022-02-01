using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Serialization;
using DiscordBot.Common.Configuration;
using Microsoft.Extensions.Options;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Helpers.Extensions;

public class MetricTypeParser {
	private readonly IOptions<MetricSynonymsConfiguration> _options;

	public MetricTypeParser(IOptions<MetricSynonymsConfiguration> options) => _options = options;

	public bool TryParseToMetricType(string metricType, out MetricType value) {
		var configuration = _options.Value;
		value = 0; // default

		// Parse by attributes
		var loweredInput = metricType.ToLowerInvariant();

		foreach (var field in typeof(MetricType).GetFields()) {
			var displayAttr = field.GetCustomAttribute<DisplayAttribute>();

			if (displayAttr is not null && displayAttr.Name is not null && displayAttr.Name.ToLowerInvariant() == loweredInput) {
				value = (MetricType)field.GetValue(null);
				return true;
			}

			var enumMemAttr = field.GetCustomAttribute<EnumMemberAttribute>();
			if (enumMemAttr is not null && enumMemAttr.Value is not null && enumMemAttr.Value.ToLowerInvariant() == loweredInput) {
				value = (MetricType)field.GetValue(null);
				return true;
			}
			
			if(field.Name.ToLowerInvariant() == loweredInput) {
				value = (MetricType)field.GetValue(null);
				return true;
			}
		}

		if (configuration == null || configuration.Synonyms == null || configuration.Synonyms.Count <= 1) {
			return false;
		}

		foreach (var kvp in configuration.Synonyms) {
			if (kvp.Value.Contains(metricType, StringComparer.InvariantCultureIgnoreCase)) {
				value = kvp.Key;
				return true;
			}
		}

		return false;
	}
}
