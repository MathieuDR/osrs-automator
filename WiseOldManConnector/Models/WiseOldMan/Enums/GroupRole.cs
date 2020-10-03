using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace WiseOldManConnector.Models.WiseOldMan.Enums {
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GroupRole {
        [EnumMember(Value = "member")]
        Member = 0,
        [EnumMember(Value = "leader")]
        Leader = 1
    }
}