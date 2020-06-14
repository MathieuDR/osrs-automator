using System.Security.Cryptography;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;

namespace DiscordBotFanatic.Models.WiseOldMan.Cleaned {
    public class DeltaInfo : BaseInfo<DeltaMetric> {
        public DeltaInfo() { }
        public DeltaInfo(string type) : base(type) { }
        public DeltaInfo(DeltaMetric info, MetricType type) : base(info, type) { }
        public DeltaInfo(DeltaMetric info, string type) : base(info, type) { }

        public bool IsRanked {
            get { return Info.Rank.End > 0; }
        }

        public Delta CorrectDelta {
            get {
                if (Type.IsSkillMetric()) {
                    return Info.Experience;
                }else if (Type.IsBossMetric()) {
                    return Info.Kills;
                } else {
                    return Info.Score;
                }
            }
        }

        public int Gained {
            get {
                return CorrectDelta.Gained;
            }
        }
    }
}