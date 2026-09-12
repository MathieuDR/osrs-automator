using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Commands;
using FluentResults;

namespace DiscordBot.Services.Interfaces; 

public interface IConfirmationService {
    Task<Result> CreateConfirm(GuildUser requestedBy, IConfirmCommand command, string hostingFooter = null, string avatarUrl = null);
    Result Confirm(bool accepted, DiscordMessageId messageId, GuildUser confirmedBy);
    Result SetConfirmChannel(Channel channelId, GuildUser requestedBy);
}
