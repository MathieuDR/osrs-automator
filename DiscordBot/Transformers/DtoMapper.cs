using DiscordBot.Common.Dtos.Discord;

namespace DiscordBot.Transformers; 

public static class DiscordMapper {
    public static GuildUser ToGuildUserDto(this IGuildUser user) {
        return new GuildUser {
            Id = user.Id,
            Username = user.DisplayName(),
            GuildId = user.GuildId
        };
    }

    public static GuildUser ToGuildUserDto(this IUser user) {
        var guildUser = (IGuildUser) user;
        return guildUser.ToGuildUserDto();
    }

    public static Guild ToGuildDto(this IGuildUser user) {
        return new Guild {
            Id = user.GuildId,
            Name = user.Guild.Name
        };
    }

    public static Guild ToGuildDto(this IGuild guild) {
        return new Guild {
            Id = guild.Id,
            Name = guild.Name
        };
    }

    public static Role ToRoleDto(this IRole role) {
        return new Role {
            Id = role.Id,
            Name = role.Name,
            Color = role.Color
        };
    }

    public static Channel ToChannelDto(this IChannel channel) {
        return new Channel {
            Id = channel.Id,
            Name = channel.Name
        };
    }

    public static Channel ToChannelDto(this IGuildChannel channel) {
        return new Channel(channel.GuildId) {
            Id = channel.Id,
            Name = channel.Name,
            IsGuildChannel = true
        };
    }

    public static Channel ToChannelDto(this ITextChannel channel) {
        return new Channel(channel.GuildId) {
            Id = channel.Id,
            Name = channel.Name,
            IsTextChannel = true,
            IsGuildChannel = true
        };
    }

    public static Channel ToChannelDto(this IDMChannel channel) {
        return new Channel(channel.Recipient.Id) {
            Id = channel.Id,
            Name = channel.Name,
            IsTextChannel = true,
            IsDMChannel = true
        };
    }
}