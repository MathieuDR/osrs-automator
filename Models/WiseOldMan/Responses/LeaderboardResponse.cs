using System.Collections.Generic;
using System.Linq;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Helpers.JsonConverters;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Cleaned;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;
using Newtonsoft.Json;

namespace DiscordBotFanatic.Models.WiseOldMan.Responses {
    [JsonConverter(typeof(LeaderboardConverter))]
    public class LeaderboardResponse : BaseResponse {
        private List<LeaderboardMemberInfo> _memberInfos;
        public List<LeaderboardMember> Members { get; set; }
        public MetricType RequestedType { get; set; }
        public Period RequestedPeriod { get; set; }

        public List<LeaderboardMemberInfo> MemberInfos {
            get { return _memberInfos ??= Members.Select(x => x.ToLeaderboardMemberInfo(RequestedType)).Where(x=>x.HasGained).ToList(); }
        }
    }
}