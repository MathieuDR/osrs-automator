using System.Collections.Generic;
using DiscordBotFanatic.Helpers.JsonConverters;
using Newtonsoft.Json;
using WiseOldManConnector.Models.API.Responses.Models;

namespace WiseOldManConnector.Models.API.Responses {
    [JsonConverter(typeof(ListConverter))]
    public class GroupMembersListResponse : BaseResponse {
        public List<GroupMember> Members { get; set; }
    }
}