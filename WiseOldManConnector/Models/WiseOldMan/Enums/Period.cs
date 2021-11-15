using System.Runtime.Serialization;

namespace WiseOldManConnector.Models.WiseOldMan.Enums;

public enum Period {
    [EnumMember(Value = "day")]
    Day,

    [EnumMember(Value = "week")]
    Week,

    [EnumMember(Value = "month")]
    Month,

    [EnumMember(Value = "year")]
    Year,

    [EnumMember(Value = "5min")]
    FiveMinutes
}
