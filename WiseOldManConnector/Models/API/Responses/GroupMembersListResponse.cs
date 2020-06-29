using System.Collections.Generic;
using Newtonsoft.Json;
using WiseOldManConnector.Helpers.JsonConverters;
using WiseOldManConnector.Models.API.Responses.Models;

namespace WiseOldManConnector.Models.API.Responses {
    [JsonConverter(typeof(GroupMemberConverter))]
    internal class GroupMembersListResponse : BaseResponse {
        public List<GroupMember> Members { get; set; }
    }
}