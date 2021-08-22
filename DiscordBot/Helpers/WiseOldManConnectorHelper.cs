using System.Collections.Generic;
using System.Linq;
using DiscordBot.Common.Helpers;
using DiscordBot.Paginator;
using WiseOldManConnector.Models.Output;

namespace DiscordBot.Helpers {
    public static class WiseOldManConnectorHelper {
        public static PaginatedStringWithContext<Player> ToPaginatedStringWithContext(this Player player) {
            return new() {Reference = player, StringValue = player.ToPlayerInfoString()};
        }

        public static IEnumerable<PaginatedStringWithContext<Player>> ToPaginatedStringWithContexts(
            this IEnumerable<Player> players) {
            return players.Select(p => p.ToPaginatedStringWithContext());
        }

        public static PaginatedStringWithContext<Competition> ToPaginatedStringWithContext(this Competition competition) {
            return new() {Reference = competition, StringValue = competition.ToCompetitionInfoString()};
        }

        public static IEnumerable<PaginatedStringWithContext<Competition>> ToPaginatedStringWithContexts(
            this IEnumerable<Competition> competitions) {
            return competitions.Select(c => c.ToPaginatedStringWithContext());
        }
    }
}
