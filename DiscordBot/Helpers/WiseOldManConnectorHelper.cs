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
        private static readonly string WiseOldManHost = "wiseoldman.net";

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
            return new PaginatedStringWithContext<Player>() {Reference = player, StringValue = player.ToPlayerInfoString()};
        }

        public static IEnumerable<PaginatedStringWithContext<Player>> ToPaginatedStringWithContexts(
            this IEnumerable<Player> players) {
            return players.Select(p => p.ToPaginatedStringWithContext());
        }

        public static PaginatedStringWithContext<Competition> ToPaginatedStringWithContext(this Competition competition) {
            return new PaginatedStringWithContext<Competition>()
                {Reference = competition, StringValue = competition.ToCompetitionInfoString()};
        }

        public static IEnumerable<PaginatedStringWithContext<Competition>> ToPaginatedStringWithContexts(
            this IEnumerable<Competition> competitions) {
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
            builder.Path = $"players/{player.DisplayName}";

            return builder.Uri.AbsoluteUri;
        }

        public static string Url(this DeltaLeaderboard deltaLeaderboard, int groupId) {
            var builder = GetWomBuilder();
            builder.Path = $"groups/{groupId}/gained";

            return builder.Uri.ToString();
        }
        
        public static IEnumerable<ItemDecorator<T>> Decorate<T>(this IEnumerable<T> items) where T : IBaseConnectorOutput {
            if (items == null) {
                return new List<ItemDecorator<T>>();
            }

            var list = items.ToList();

            return list.Select(i => i.Decorate());
        }

        public static ItemDecorator<Group> Decorate(this Group group)
        {
            return group == null ? null : new ItemDecorator<Group>(group, group.Name, group.Url());
        }

        public static ItemDecorator<Competition> Decorate(this Competition competition)
        {
            return competition == null ? null : new ItemDecorator<Competition>(competition, competition.Title, competition.Url());
        }
        
        public static ItemDecorator<Player> Decorate(this Player player)
        {
            return player == null ? null : new ItemDecorator<Player>(player, player.DisplayName, player.Url());
        }

        public static ItemDecorator<T> Decorate<T>(this T item) where T : IBaseConnectorOutput {
            switch (item) {
                case Player player:
                    return player.Decorate() as ItemDecorator<T>;
                case Group group:
                    return group.Decorate() as ItemDecorator<T>;
                case Competition competition:
                    return competition.Decorate() as ItemDecorator<T>;
                default:
                    throw new ArgumentOutOfRangeException(nameof(item));
            }
        }

        public static ItemDecorator<Leaderboard> Decorate(this DeltaLeaderboard deltaLeaderboard, int groupId, string groupName) {
            if (deltaLeaderboard == null)
            {
                return null;
            }

            Group group = new Group() { Id = groupId };
            return new ItemDecorator<Leaderboard>(deltaLeaderboard, groupName, group.Url());
        }

        public static ItemDecorator<Leaderboard> DecorateLeaderboard(this Competition competition) {
            return competition == null ? null : new ItemDecorator<Leaderboard>(competition.Leaderboard, competition.Title, competition.Url());
        }


        private static UriBuilder GetWomBuilder() {
            return new UriBuilder("https", WiseOldManHost);
        }
    }
}