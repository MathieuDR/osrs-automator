using System.Collections.Generic;
using DiscordBotFanatic.Models.Enums;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBotFanatic.Models.Configuration {
    public class MetricSynonymsConfiguration {
        public Dictionary<MetricType, List<string>> Data { get; set; }
    }
}