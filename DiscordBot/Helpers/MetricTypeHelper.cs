using System;
using System.Collections.Generic;
using System.Linq;
using DiscordBot.Common.Configuration;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Helpers {
    public static class MetricTypeHelper {
        public static bool TryParseToMetricType(this string metricType, MetricSynonymsConfiguration configuration,
            out object value) {
            if (Enum.TryParse(typeof(MetricType), metricType, true, out value)) {
                return true;
            }

            if (configuration != null && configuration.Data != null && configuration.Data.Count > 1) {
                foreach (KeyValuePair<MetricType, List<string>> kvp in configuration.Data) {
                    if (kvp.Value.Contains(metricType, StringComparer.InvariantCultureIgnoreCase)) {
                        value = kvp.Key;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}