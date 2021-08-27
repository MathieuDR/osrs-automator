using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WiseOldManConnector.Models.Requests {
    public class CreateGroupRequest {
        public CreateGroupRequest(string name) : this(name, null, null) { }

        public CreateGroupRequest(string name, string clanChat) : this(name, clanChat, null) { }

        public CreateGroupRequest(string name, IEnumerable<MemberRequest> members) : this(name, null, members) { }

        public CreateGroupRequest(string name, string clanChat, IEnumerable<MemberRequest> members) {
            if (string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException("name cannot be null or empty");
            }

            Name = name;
            ClanChat = clanChat;
            Members = members;
        }


        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("clanChat")]
        public string ClanChat { get; set; }

        [JsonProperty("members")]
        public IEnumerable<MemberRequest> Members { get; set; }
    }
}
