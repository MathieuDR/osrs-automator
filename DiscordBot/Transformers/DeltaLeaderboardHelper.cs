using System;
using System.Text;
using DiscordBot.Helpers;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Transformers {
    public static class DeltaLeaderboardHelper {
        private static readonly int IdHash = 3;
        private static readonly int IdSpace = 5;
        private static readonly int NameSpace = 15;
        private static readonly int NumberSpace = 6;

        public static string MembersToString(this DeltaLeaderboard source, int maxMembers = 20) {
            var description = new StringBuilder();
            description.Append("#".PadLeft(IdHash).PadRight(IdSpace));
            description.Append("Name".PadRight(NameSpace));
            switch (source.MetricType.Category()) {
                case MetricTypeCategory.Skills:
                    description.Append("Experience");
                    break;
                case MetricTypeCategory.Bosses:
                    description.Append("Kills/Completions");
                    break;
                case MetricTypeCategory.Activities:
                    description.Append("Score");
                    break;
                case MetricTypeCategory.Time:
                    description.Append("Hours");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            description.Append(Environment.NewLine);


            var bound = Math.Min(source.Members.Count, maxMembers);

            for (var i = 0; i < bound; i++) {
                var member = source.Members[i];

                if (member.Delta.Gained<=0) {
                    break;
                }

                description.Append($"{i + 1}, ".PadLeft(IdSpace));
                description.Append(member.Player.DisplayName.PadRight(NameSpace));
                switch (source.MetricType.Category()) {
                    case MetricTypeCategory.Time:
                        description.Append(member.Delta.Gained.FormatHours().PadLeft(NumberSpace) + Environment.NewLine);
                        break;
                    default:
                        description.Append(member.Delta.Gained.FormatNumber().PadLeft(NumberSpace) + Environment.NewLine);
                        break;     
                }
                
            }

            return description.ToString();
        }
        
        public static string MembersToString(this HighscoreLeaderboard source, int maxMembers = 20) {
            var description = new StringBuilder();
            description.Append("#".PadLeft(IdHash).PadRight(IdSpace));
            description.Append("Name".PadRight(NameSpace));
            switch (source.MetricType.Category()) {
                case MetricTypeCategory.Skills:
                    description.Append("Experience");
                    break;
                case MetricTypeCategory.Bosses:
                    description.Append("Kills/Completions");
                    break;
                case MetricTypeCategory.Activities:
                    description.Append("Score");
                    break;
                case MetricTypeCategory.Time:
                    description.Append("Hours");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            description.Append(Environment.NewLine);


            var bound = Math.Min(source.Members.Count, maxMembers);

            for (var i = 0; i < bound; i++) {
                var member = source.Members[i];

                description.Append($"{i + 1}, ".PadLeft(IdSpace));
                description.Append(member.Player.DisplayName.PadRight(NameSpace));
                switch (source.MetricType.Category()) {
                    case MetricTypeCategory.Time:
                        description.Append(member.Metric.Value.FormatHours().PadLeft(NumberSpace) + Environment.NewLine);
                        break;
                    default:
                        description.Append(member.Metric.Value.FormatNumber().PadLeft(NumberSpace) + Environment.NewLine);
                        break;     
                }
                
            }

            return description.ToString();
        }
    }
}
