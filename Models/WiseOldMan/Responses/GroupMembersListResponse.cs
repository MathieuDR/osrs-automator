using System.Collections.Generic;
using DiscordBotFanatic.Helpers.JsonConverters;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;
using Newtonsoft.Json;

namespace DiscordBotFanatic.Models.WiseOldMan.Responses {
    [JsonConverter(typeof(GroupMemberConverter))]
    public class GroupMembersListResponse : BaseResponse {
        public List<GroupMember> Members { get; set; }
    }
}