using System;
using System.Diagnostics;
using System.Linq;
using Common.Extensions;
using DiscordBot.Common.Configuration;
using Microsoft.Extensions.Options;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Helpers.Extensions {
    public class MetricTypeParser {
        private readonly IOptions<MetricSynonymsConfiguration> _options;

        public MetricTypeParser(IOptions<MetricSynonymsConfiguration> options) {
            _options = options;
        }
        public bool TryParseToMetricType(string metricType, out MetricType value) {
            var configuration = _options.Value;
            
            if (Enum.TryParse<MetricType>(metricType, true, out value)) {
                return true;
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
}
