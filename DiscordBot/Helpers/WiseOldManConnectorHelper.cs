using System.Text;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBotFanatic.Helpers {
    public static class WiseOldManConnectorHelper {
        public static string ToPlayerInfoString(this Player player) {
            StringBuilder builder = new StringBuilder();
            builder.Append($"{player.DisplayName} - Combat ");
            builder.Append(player.CombatLevel < 3 ? $"3" : $"{player.CombatLevel}");

            if (player.LatestSnapshot != null && player.LatestSnapshot.GetMetricForType(MetricType.Overall) != null) {
                builder.Append($" - Overall lvl {player.LatestSnapshot.GetMetricForType(MetricType.Overall).Level}");
            }

            return builder.ToString();
        }
    }
}