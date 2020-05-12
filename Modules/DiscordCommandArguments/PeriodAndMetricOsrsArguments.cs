using DiscordBotFanatic.Models.Enums;

namespace DiscordBotFanatic.Modules.DiscordCommandArguments {

    public class PeriodAndMetricOsrsArguments : BaseOsrsArguments{
        public Period? Period { get; set; }
        public MetricType? MetricType { get; set; }
    }
}