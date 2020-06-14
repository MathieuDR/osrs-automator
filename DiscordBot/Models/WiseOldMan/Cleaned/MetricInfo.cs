using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;

namespace DiscordBotFanatic.Models.WiseOldMan.Cleaned {
    public class MetricInfo : BaseInfo<Metric> {
        public MetricInfo() { }
        public MetricInfo(string type) : base(type) { }
        public MetricInfo(Metric info, MetricType type) : base(info, type) { }
        public MetricInfo(Metric info, string type) : base(info, type) { }

        public bool IsRanked {
            get { return Info.Rank > 0; }
        }

        public int CorrectScore{
            get {
                if (Type.IsSkillMetric()) {
                    return Info.Experience;
                }else if (Type.IsBossMetric()) {
                    return 0;  //Info.Kills;
                } else {
                    return 0;  //Info.Score;
                }
            }
        }
    }
}