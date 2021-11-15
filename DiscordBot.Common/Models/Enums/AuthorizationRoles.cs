using System.Runtime.Serialization;

namespace DiscordBot.Common.Models.Enums; 

[Flags]
public enum AuthorizationRoles {
    BotOwner = 1,
    BotAdmin = 2,
    BotModerator = 4,
    
    ClanOwner = 8, // Server owner
    
    [EnumMember(Value = "Clan administrator")]
    ClanAdmin = 16,
    
    [EnumMember(Value = "Clan moderator")]
    ClanModerator = 32,
    
    [EnumMember(Value = "Clan event host")]
    ClanEventHost = 64,
    
    [EnumMember(Value = "Clan participant")]
    ClanEventParticipant = 128,
    
    [EnumMember(Value = "Clan member")]
    ClanMember = 256,
    
    [EnumMember(Value = "Server guest")]
    ClanGuest = 512,
    
    None = 1024
}
