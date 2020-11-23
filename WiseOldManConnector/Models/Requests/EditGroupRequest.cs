using System.Collections.Generic;
using Newtonsoft.Json;
using WiseOldManConnector.Interfaces;

namespace WiseOldManConnector.Models.Requests {
    public class EditGroupRequest : CreateGroupRequest, IVerifiable {
        public EditGroupRequest(string verificationCode, string name) : this(verificationCode, name, null, null) { }
        public EditGroupRequest(string verificationCode, string name, string clanChat) : this(verificationCode, name, clanChat, null) { }
        public EditGroupRequest(string verificationCode, string name, IEnumerable<MemberRequest> members) : this(verificationCode, name, null, members) { }

        public EditGroupRequest(string verificationCode, string name, string clanChat, IEnumerable<MemberRequest> members) : base(
            name, clanChat, members) {
            VerificationCode = verificationCode;
        }

        [JsonProperty("verificationCode")]
        public string VerificationCode { get; set; }
    }
}