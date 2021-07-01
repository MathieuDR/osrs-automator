using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Modules.DiscordCommandArguments {
    public class PeriodAndMetricArguments : BaseArguments {
        public Period? Period { get; set; }
        public MetricType? MetricType { get; set; }
    }
}