using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WiseOldManConnector.Models.WiseOldMan.Enums;

public enum PlayerType {
    [Display(Name = "Player")]
    Unknown = 0,

    [EnumMember(Value = "regular")]
    [Display(Name = "Regular")]
    Regular = 1,

    [EnumMember(Value = "ironman")]
    [Display(Name = "Ironman")]
    IronMan,

    [EnumMember(Value = "hardcore")]
    [Display(Name = "Hardcore Ironman")]
    HardcoreIronMan,

    [EnumMember(Value = "ultimate")]
    [Display(Name = "Ultimate Ironman")]
    UltimateIronMan,
    
    [EnumMember(Value = "group_ironman")]
    [Display(Name = "Group Ironman")]
    GroupIronMan,
    
    [EnumMember(Value = "fresh_start")]
    [Display(Name = "Fresh start")]
    FreshStart
}
