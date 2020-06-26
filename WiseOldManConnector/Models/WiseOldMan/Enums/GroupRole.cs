using System.Runtime.Serialization;

namespace WiseOldManConnector.Models.WiseOldMan.Enums {
    public enum GroupRole {
        [EnumMember(Value = "member")]
        Member = 0,
        [EnumMember(Value = "leader")]
        Leader = 1
    }
}