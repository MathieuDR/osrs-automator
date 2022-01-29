using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace DiscordBot.Common.Models.Enums;

[Flags]
public enum AuthorizationRoles {
    [EnumMember(Value = "Bot owner")]
    [Display(Name = "Bot owner")]
    BotOwner = 1,
    
    [EnumMember(Value = "Bot admin")]
    [Display(Name = "Bot admin")]
    BotAdmin = 2,
    
    [EnumMember(Value = "Bot moderator")]
    [Display(Name = "Bot moderator")]
    BotModerator = 4,
    
    [EnumMember(Value = "Server owner")]
    [Display(Name = "Server Owner")]
    ClanOwner = 8, // Server owner

    [EnumMember(Value = "Clan administrator")]
    [Display(Name = "Clan administrator")]
    ClanAdmin = 16,

    [EnumMember(Value = "Clan moderator")]
    [Display(Name = "Clan moderator")]
    ClanModerator = 32,

    [EnumMember(Value = "Clan event host")]
    [Display(Name = "Clan event host")]
    ClanEventHost = 64,

    [EnumMember(Value = "Clan participant")]
    [Display(Name = "Clan participant")]
    ClanEventParticipant = 128,

    [EnumMember(Value = "Clan member")]
    [Display(Name = "Clan member")]
    ClanMember = 256,

    [EnumMember(Value = "Server guest")]
    [Display(Name = "Server guest")]
    ClanGuest = 512,

    [EnumMember(Value = "Server member")]
    [Display(Name = "Server member")]
    None = 1024
}
