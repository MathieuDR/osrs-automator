using System.Collections.Generic;
using System.Linq;
using DiscordBotFanatic.Models.WiseOldMan.Cleaned;

namespace DiscordBotFanatic.Helpers {
    public static class InfoHelper {
        public static List<DeltaInfo> RemoveEmpty(this List<DeltaInfo> list) {
            return list.Where(x => x.IsRanked).ToList();
        }

        public static List<DeltaInfo> RemoveNegativeInfos(this List<DeltaInfo> list) {
            return list.Where(x => x.Gained > 0).ToList();
        }

        public static List<MetricInfo> RemoveEmpty(this List<MetricInfo> list) {
            return list.Where(x => x.IsRanked).ToList();
        }

        public static bool IsSamePeriod(this List<RecordInfo> list) {
            // Is 0 periods the same amount? not sure.
            int periods = list.Select(x => x.Info.Period).Distinct().Count();
            return  periods<= 1;
        }

        public static List<RecordInfo> RemoveEmpty(this List<RecordInfo> list) {
            return list.Where(x => !x.IsEmptyRecord).ToList();
        }

        public static List<LeaderboardMemberInfo> RemoveEmpty(this List<LeaderboardMemberInfo> list) {
            return list.Where(x => !x.HasGained).ToList();
        }
    }
}