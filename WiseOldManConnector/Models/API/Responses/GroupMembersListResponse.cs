using System.Collections.Generic;
using Newtonsoft.Json;
using WiseOldManConnector.Models.API.Responses.Models;

namespace WiseOldManConnector.Models.API.Responses {
    [JsonConverter(typeof(GroupMemberConverter))]
    public class GroupMembersListResponse : BaseResponse {
        public List<GroupMember> Members { get; set; }
    }
}