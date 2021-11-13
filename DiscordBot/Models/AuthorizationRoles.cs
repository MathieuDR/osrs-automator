namespace DiscordBot.Models; 

[Flags]
public enum AuthorizationRoles {
    BotOwner = 1,
    BotAdmin = 2,
    BotModerator = 4,
    ClanOwner = 8,
    ClanAdmin = 16,
    ClanModerator = 32,
    ClanEventHost = 64,
    ClanEventParticipant = 128,
    ClanMember = 256,
    ClanGuest = 512,
}
