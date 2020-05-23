using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;

namespace DiscordBotFanatic.Models.WiseOldMan.Cleaned {
    public class MetricInfo : BaseInfo {
        public MetricInfo() { }

        public MetricInfo(string type) : base(type) { }

        public MetricInfo(Metric metric, MetricType type) {
            Metric = metric;
            Type = type;
        }

        public MetricInfo(Metric metric, string type) : this(metric, type.ToMetricType()) { }

        public Metric Metric { get; set; }
    }
}