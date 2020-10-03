using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WiseOldManConnector.Helpers.JsonConverters;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Responses {
    [JsonConverter(typeof(LeaderboardConverter))]
    internal class LeaderboardResponse : BaseResponse {
        public List<LeaderboardMember> Members { get; set; }
        public MetricType RequestedType { get; set; }
        public Period RequestedPeriod { get; set; }
    }
}