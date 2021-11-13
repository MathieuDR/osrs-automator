namespace DiscordBot.Common.Models.Enums; 

[Flags]
public enum AuthorizationRoles {
    BotOwner = 1,
    BotAdmin = 2,
    BotModerator = 4,
    ClanOwner = 8, // Also the server owner
    ClanAdmin = 16,
    ClanModerator = 32,
    ClanEventHost = 64,
    ClanEventParticipant = 128,
    ClanMember = 256,
    ClanGuest = 512,
    None = 1024
}
