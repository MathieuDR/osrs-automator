using System;
using System.Collections.Generic;
using System.Globalization;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Cleaned;
using DiscordBotFanatic.Models.WiseOldMan.Responses;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;

namespace DiscordBotFanatic.Helpers {
    public static class MetricHelper {

        public static MetricInfo ToMetricInfo(this Metric metric, MetricType type) {
            return new MetricInfo(metric, type);
        }
        public static MetricInfo ToMetricInfo(this Metric metric, string type) {
            return new MetricInfo(metric, type);
        }

        public static DeltaInfo ToDeltaInfo(this DeltaMetric metric, MetricType type) {
            return new DeltaInfo(metric, type);
        }
        public static DeltaInfo ToDeltaInfo(this DeltaMetric metric, string type) {
            return new DeltaInfo(metric, type);
        }

        public static RecordInfo ToRecordInfo(this Record record) {
            return new RecordInfo(record);
        }

        public static LeaderboardMemberInfo ToLeaderboardMemberInfo(this LeaderboardMember leaderboardMember, MetricType requestedType) {
            return new LeaderboardMemberInfo(leaderboardMember, requestedType);
        }

        public static List<CompetitionInfo> ToCompetitionInfos(this CompetitionResponse competitionResponse) {
            var result = new List<CompetitionInfo>();

            var type = competitionResponse.Metric.ToMetricType();
            foreach (Participant participant in competitionResponse.Participants) {
                var info = new CompetitionInfo(competitionResponse.Title, competitionResponse.StartsAt, competitionResponse.EndsAt, competitionResponse.Group, participant, type);
                result.Add(info);
            }

            return result;
        }
        
        public static string ToLevel(this Metric metric) {
            return metric.Experience.ToLevel().ToString();
        }

        public static string FormattedExperience(this Metric metric) {
            return metric.Experience.FormatNumber();
        }

        public static string FormattedRank(this Metric metric) {
            return metric.Rank.FormatNumber();
        }

        public static string StartLevel(this DeltaMetric metric) {
            return metric.Experience.Start.ToLevel().ToString();
        }

        public static string EndLevel(this DeltaMetric metric) {
            return metric.Experience.End.ToLevel().ToString();
        }

            public static string LevelGained(this DeltaMetric metric) {
                return (metric.Experience.End.ToLevel() - metric.Experience.Start.ToLevel()).FormatConditionally();
        }

        public static string StartExperience(this DeltaMetric metric) {
            return metric.Experience.Start.FormatNumber();
        }

        public static string EndExperience(this DeltaMetric metric) {
            return metric.Experience.Start.FormatNumber();
        }

        public static string GainedExperience(this DeltaMetric metric) {
            return metric.Experience.Gained.FormatConditionally();
        }

        public static string StartRank(this DeltaMetric metric) {
            return metric.Rank.Start.FormatNumber();
        }

        public static string EndRank(this DeltaMetric metric) {
            return metric.Rank.End.FormatNumber();
        }

        public static string GainedRanks(this DeltaMetric metric) {
            return (metric.Rank.Start - metric.Rank.End).FormatConditionally();
        }

        public static MetricType ToMetricType(this string metricType) {
            if(Enum.TryParse(metricType, true, out MetricType result)) {
                return result;
            }

            throw new ArgumentOutOfRangeException($"{metricType} is not a metric type.");
        }

        public static string FormatNumber(this int number) {
            var nfi = (NumberFormatInfo) CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";

            if (number >= 100000000) {
                return (number / 1000000).ToString("#,0M", nfi);
            }

            if (number >= 10000000) {
                return (number / 1000000).ToString("0.#", nfi) + "M";
            }

            if (number >= 100000) {
                return (number / 1000).ToString("#,0K", nfi);
            }

            if (number >= 10000) {
                return (number / 1000).ToString("0.#", nfi) + "K";
            }

            return number.ToString("#,#", nfi);
        }

        public static string FormatConditionally(this int number) {
            var nfi = (NumberFormatInfo) CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = " ";

            if (number >= 100000000) {
                return (number / 1000000).ToString("+#,0M;-#,0M", nfi);
            }

            if (number >= 10000000) {
                return (number / 1000000).ToString("+0.#;-0.#", nfi) + "M";
            }

            if (number >= 100000) {
                return (number / 1000).ToString("+#,0K;-#,0K", nfi);
            }

            if (number >= 10000) {
                return (number / 1000).ToString("+0.#;-0.#", nfi) + "K";
            }

            return number.ToString("+#;-#;0", nfi);
        }

      

        public static int ToLevel(this int experience) {
            int index;

            for (index = 0; index < Experiences.Length; index++) {
                if (index + 1 == Experiences.Length) {
                    break;
                }

                if (Experiences[index + 1] > experience) {
                    break;
                }
            }

            return index;
        }

        private static readonly int[] Experiences = {
            0, 0, 83, 174, 276, 388, 512, 650, 801, 969, 1154, 1358, 1584, 1833, 2107, 2411, 2746, 3115, 3523, 3973,
            4470, 5018, 5624, 6291, 7028, 7842, 8740, 9730, 10824, 12031, 13363, 14833, 16456, 18247, 20224, 22406,
            24815, 27473, 30408, 33648, 37224, 41171, 45529, 50339, 55649, 61512, 67983, 75127, 83014, 91721, 101333,
            111945, 123660, 136594, 150872, 166636, 184040, 203254, 224466, 247886, 273742, 302288, 333804, 368599,
            407015, 449428, 496254, 547953, 605032, 668051, 737627, 814445, 899257, 992895, 1096278, 1210421, 1336443,
            1475581, 1629200, 1798808, 1986068, 2192818, 2421087, 2673114, 2951373, 3258594, 3597792, 3972294, 4385776,
            4842295, 5346332, 5902831, 6517253, 7195629, 7944614, 8771558, 9684577, 10692629, 11805606, 13034431, 14391160,
            15889109, 17542976, 19368992, 21385073, 23611006, 26068632, 28782069, 31777943, 35085654, 38737661, 42769801, 47221641,
            52136869, 57563718, 63555443, 70170840, 77474828, 85539082, 94442737, 104273167, 115126838, 127110260, 140341028, 154948977,
            171077457, 188884740, 200000000
        };
    }
}