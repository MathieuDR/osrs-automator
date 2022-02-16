using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Identities;

namespace DiscordBot.Transformers;

public static class DiscordMapper {
    public static GuildUser ToGuildUserDto(this IGuildUser user) {
        return new GuildUser {
            Id = new DiscordUserId(user.Id),
            Username = user.DisplayName(),
            GuildId = new DiscordGuildId(user.GuildId)
        };
    }

    public static GuildUser ToGuildUserDto(this IUser user) {
        var guildUser = (IGuildUser)user;
        return guildUser.ToGuildUserDto();
    }

    public static Guild ToGuildDto(this IGuildUser user) {
        return new Guild {
            GuildId = new DiscordGuildId(user.GuildId),
            Name = user.Guild.Name
        };
    }

    public static Guild ToGuildDto(this IGuild guild) {
        return new Guild {
            GuildId = new DiscordGuildId(guild.Id),
            Name = guild.Name
        };
    }

    public static Role ToRoleDto(this IRole role) {
        return new Role {
            Id = new DiscordRoleId(role.Id),
            Name = role.Name,
            Color = role.Color,
            GuildId =  new DiscordGuildId(role.Guild.Id)
        };
    }

    public static Channel ToChannelDto(this IChannel channel) {
        return channel switch {
            IDMChannel dmChannel => dmChannel.ToChannelDto(),
            ITextChannel textChannel => textChannel.ToChannelDto(),
            IGuildChannel guildChannel => guildChannel.ToChannelDto(),
            _ => new Channel {
                Id = new DiscordChannelId(channel.Id),
                Name = channel.Name,
            }
        };
    }

    public static Channel ToChannelDto(this IGuildChannel channel) {
        return new Channel() {
            Id =  new DiscordChannelId(channel.Id),
            Name = channel.Name,
            Guild = channel.Guild.ToGuildDto()
        };
    }

    public static Channel ToChannelDto(this ITextChannel channel) {
        return new Channel() {
            Id = new DiscordChannelId(channel.Id),
            Name = channel.Name,
            IsTextChannel = true,
            Guild = channel.Guild.ToGuildDto()
        };
    }

    public static Channel ToChannelDto(this IDMChannel channel) {
        return new Channel() {
            Id = new DiscordChannelId(channel.Id),
            RecipientId = new DiscordUserId(channel.Recipient.Id),
            Name = channel.Name,
            IsTextChannel = true,
            IsDMChannel = true
        };
    }

    public static Message ToPostDto(this IMessage message) {
        return new Message() {
            Id = new DiscordMessageId(message.Id),
            Channel = message.Channel.ToChannelDto(),
            AuthorId = new DiscordUserId(message.Author.Id)
        };
    }
}
