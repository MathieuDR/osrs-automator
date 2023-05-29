using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using DiscordBot.Common.JsonConverters;


namespace DiscordBot.Dashboard.Models.Dink; 

[JsonConverter(typeof(EnumAttributeConverter<DinkType>))]
internal enum DinkType {
    [EnumMember(Value = "DEATH")]
    Death,
    [EnumMember(Value = "COLLECTION")]
    Collection,
    [EnumMember(Value = "LEVEL")]
    Level,
    [EnumMember(Value = "LOOT")]
    Loot,
    [EnumMember(Value = "SLAYER")]
    Slayer,
    [EnumMember(Value = "QUEST")]
    Quest,
    [EnumMember(Value = "CLUE")]
    Clue,
    [EnumMember(Value = "KILL_COUNT")]
    KillCount,
    [EnumMember(Value = "COMBAT_ACHIEVEMENT")]
    CombatAchievement,
    [EnumMember(Value = "ACHIEVEMENT_DIARY")]
    AchievementDiary,
    [EnumMember(Value = "PET")]
    Pet,
    [EnumMember(Value = "SPEEDRUN")]
    SpeedRun,
    [EnumMember(Value = "BARBARIAN_ASSAULT_GAMBLE")]
    BarbarianAssaultGamble,
    [EnumMember(Value = "PLAYER_KILL")]
    PlayerKill,
    [EnumMember(Value = "GROUP_STORAGE")]
    GroupStorage
}
