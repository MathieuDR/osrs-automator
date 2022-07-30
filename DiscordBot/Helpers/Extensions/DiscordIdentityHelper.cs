using DiscordBot.Common.Identities;

namespace DiscordBot.Helpers.Extensions; 

public static class DiscordIdentityHelper {
    
    public static DiscordGuildId GetGuildId(this IGuild guild) {
        return new DiscordGuildId(guild.Id);
    }
    
    public static DiscordGuildId GetGuildId(this IGuildUser guildUser) {
        return new DiscordGuildId(guildUser.GuildId);
    }
    
    public static DiscordUserId GetUserId(this IUser user) {
        return new DiscordUserId(user.Id);
    }
    
    public static DiscordRoleId GetRoleId(this IRole role) {
        return new DiscordRoleId(role.Id);
    }
    
    public static DiscordMessageId GetMessageId(this IMessage message) {
        return new DiscordMessageId(message.Id);
    }
}
