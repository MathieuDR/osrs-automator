using System.Runtime.Serialization;

namespace WiseOldManConnector.Models.WiseOldMan.Enums; 

public enum PlayerType {
    Unknown = 0,

    [EnumMember(Value = "regular")]
    Regular = 1,

    [EnumMember(Value = "ironman")]
    IronMan,

    [EnumMember(Value = "hardcore")]
    HardcoreIronMan,

    [EnumMember(Value = "ultimate")]
    UltimateIronMan
}