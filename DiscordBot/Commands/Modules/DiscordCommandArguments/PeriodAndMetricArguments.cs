using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Commands.Modules.DiscordCommandArguments {
    public class PeriodAndMetricArguments : BaseArguments {
        public Period? Period { get; set; }
        public MetricType? MetricType { get; set; }
    }
}
