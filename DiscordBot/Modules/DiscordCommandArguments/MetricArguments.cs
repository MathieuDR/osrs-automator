using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Modules.DiscordCommandArguments {
    public class MetricArguments : BaseArguments {
        public MetricType? MetricType { get; set; }
    }
}
