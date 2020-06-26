using System.Collections.Generic;
using System.Linq;
using DiscordBotFanatic.Helpers.JsonConverters;
using Newtonsoft.Json;
using WiseOldManConnector.Models.API.Responses.Models;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Responses {
    [JsonConverter(typeof(LeaderboardConverter))]
    internal class LeaderboardResponse : BaseResponse {
        private List<LeaderboardMemberInfo> _memberInfos;
        public List<LeaderboardMember> Members { get; set; }
        public MetricType RequestedType { get; set; }
        public Period RequestedPeriod { get; set; }

        public List<LeaderboardMemberInfo> MemberInfos {
            get { return _memberInfos ??= Members.Select(x => x.ToLeaderboardMemberInfo(RequestedType)).Where(x=>x.HasGained).ToList(); }
        }
    }
}