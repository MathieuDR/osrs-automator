using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiscordBotFanatic.Paginator;
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

        public static string ToCompetitionInfoString(this Competition competition) {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Competition: {competition.Title} ({competition.Metric})");
            builder.AppendLine($"Start: {competition.StartDate}");
            builder.AppendLine($"End: {competition.EndDate}");
            builder.AppendLine($"Duration: {competition.Duration}");

            return builder.ToString();
        }

        public static PaginatedStringWithContext<Player> ToPaginatedStringWithContext(this Player player) {
            return new PaginatedStringWithContext<Player>(){Reference = player, StringValue = player.ToPlayerInfoString()};
        }

        public static IEnumerable<PaginatedStringWithContext<Player>> ToPaginatedStringWithContexts(this IEnumerable<Player> players) {
            return players.Select(p => p.ToPaginatedStringWithContext());
        }

        public static PaginatedStringWithContext<Competition> ToPaginatedStringWithContext(this Competition competition) {
            return new PaginatedStringWithContext<Competition>(){Reference = competition, StringValue = competition.ToCompetitionInfoString()};
        }

        public static IEnumerable<PaginatedStringWithContext<Competition>> ToPaginatedStringWithContexts(this IEnumerable<Competition> competitions) {
            return competitions.Select(c => c.ToPaginatedStringWithContext());
        }
    }
}