using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;

namespace DiscordBotFanatic.Models.WiseOldMan.Cleaned {
    public abstract class BaseInfo {
        public BaseInfo() { }

        public BaseInfo(string type) {
            Type = type.ToMetricType();
        }
        public MetricType Type { get; set; }
    }
}