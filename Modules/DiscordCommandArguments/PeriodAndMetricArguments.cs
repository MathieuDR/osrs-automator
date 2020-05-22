using DiscordBotFanatic.Models.Enums;

namespace DiscordBotFanatic.Modules.DiscordCommandArguments {

    public class PeriodAndMetricArguments : BaseArguments{
        public Period? Period { get; set; }
        public MetricType? MetricType { get; set; }
    }
}