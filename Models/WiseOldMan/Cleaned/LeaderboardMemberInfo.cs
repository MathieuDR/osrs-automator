using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;

namespace DiscordBotFanatic.Models.WiseOldMan.Cleaned {
    public class LeaderboardMemberInfo : BaseInfo<LeaderboardMember> {
        public LeaderboardMemberInfo() { }
        public LeaderboardMemberInfo(LeaderboardMember info, MetricType type) : base(info, type) { }
        public LeaderboardMemberInfo(LeaderboardMember info, string type) : base(info, type) { }

        public bool HasGained {
            get {
                return Info.Gained > 0;
            }
        }
    }
}