using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data.Base;

namespace DiscordBot.Common.Models.Data.Confirmation; 

public sealed record ConfirmationConfiguration : BaseGuildRecord {
    public ConfirmationConfiguration() {
    }

    public ConfirmationConfiguration(DiscordChannelId confirmationChannel) => ConfirmationChannel = confirmationChannel;

    public ConfirmationConfiguration(GuildUser user, DiscordChannelId confirmationChannel) : base(user.GuildId, user.Id) => ConfirmationChannel = confirmationChannel;

    public DiscordChannelId ConfirmationChannel { get; set; }
}
