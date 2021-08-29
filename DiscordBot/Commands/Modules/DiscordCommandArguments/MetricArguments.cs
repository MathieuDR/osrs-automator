using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Commands.Modules.DiscordCommandArguments {
    public class MetricArguments : BaseArguments {
        public MetricType? MetricType { get; set; }
    }
}
