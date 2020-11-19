using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DiscordBotFanatic.Models.Decorators;
using DiscordBotFanatic.Paginator;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBotFanatic.Helpers {
    public static class WiseOldManConnectorHelper {

        private static string _wiseOldManHost = "wiseoldman.net";
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

        public static string Url(this Group group) {
            var builder = GetWomBuilder();
            builder.Path = $"groups/{group.Id}";

            return builder.Uri.ToString();
        }

        public static string Url(this Competition competition) {
            var builder = GetWomBuilder();
            builder.Path = $"competitions/{competition.Id}";

            return builder.Uri.ToString();
        }


        public static string Url(this Player player) {
            var builder = GetWomBuilder();
            builder.Path = $"players/{player.Id}";

            return builder.Uri.ToString();
        }

        public static string Url(this DeltaLeaderboard deltaLeaderboard, int groupId) {
            var builder = GetWomBuilder();
            builder.Path = $"groups/{groupId}/gained";

            return builder.Uri.ToString();
        }

        public static ItemDecorator<Group> Decorate(this Group group) {
            return new ItemDecorator<Group>(group, group.Name, group.Url());
        }

        public static ItemDecorator<Competition> Decorate(this Competition competition) {
            return new ItemDecorator<Competition>(competition, competition.Title, competition.Url());
        }

        public static ItemDecorator<Leaderboard> DecorateLeaderboard(this Competition competition) {
            return new ItemDecorator<Leaderboard>(competition.Leaderboard, competition.Title, competition.Url());
        }

        public static ItemDecorator<DeltaLeaderboard> Decorate(this DeltaLeaderboard deltaLeaderboard, int groupId, string groupName) {
            Group group = new Group(){Id = groupId}; // oopsiedaisy. Should make it neat-o
            return new ItemDecorator<DeltaLeaderboard>(deltaLeaderboard, groupName, group.Url());
        }

        public static ItemDecorator<DeltaLeaderboard> Decorate(this DeltaLeaderboard deltaLeaderboard, Group group) {
            return deltaLeaderboard.Decorate(group.Id, group.Name);
        }

        public static ItemDecorator<Leaderboard> DecorateGeneric(this DeltaLeaderboard deltaLeaderboard, int groupId, string groupName) {
            Group group = new Group(){Id = groupId}; // oopsiedaisy. Should make it neat-o
            return new ItemDecorator<Leaderboard>(deltaLeaderboard, groupName, group.Url());
        }

        public static ItemDecorator<Leaderboard> DecorateGeneric(this DeltaLeaderboard deltaLeaderboard, Group group) {
            return deltaLeaderboard.DecorateGeneric(group.Id, group.Name);
        }

        public static IEnumerable<ItemDecorator<Group>> Decorate(this IEnumerable<Group> groups) {
            return groups.Select(g => g.Decorate());
        }

        public static IEnumerable<ItemDecorator<Competition>> Decorate(this IEnumerable<Competition> competitions) {
            return competitions.Select(c => c.Decorate());
        }

        public static IEnumerable<ItemDecorator<Leaderboard>> DecorateLeaderboard(this IEnumerable<Competition> competitions) {
            return competitions.Select(c => c.DecorateLeaderboard());
        }

        public static ItemDecorator<Player> Decorate(this Player player) {
            return new ItemDecorator<Player>(player, player.DisplayName, player.Url());
        }

        public static IEnumerable<ItemDecorator<Player>> Decorate(this IEnumerable<Player> players) {
            return players.Select(p => p.Decorate());
        }


        private static UriBuilder GetWomBuilder() {
            return new UriBuilder("https",_wiseOldManHost);
        }
    }
}