using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Commands;
using DiscordBot.Common.Models.Data.Confirmation;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Interfaces;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Services;

internal sealed class ConfirmationService : RepositoryService, IConfirmationService {
    private readonly IMediator _mediator;
    private readonly IDiscordService _discordService;
    public ConfirmationService(IMediator mediator, IDiscordService discordService, ILogger<ConfirmationService> logger, IRepositoryStrategy repositoryStrategy) : base(logger, repositoryStrategy) {
        _mediator = mediator;
        _discordService = discordService;
    }

    public async Task<Result> CreateConfirm(GuildUser requestedBy, IConfirmCommand command) {
        var configResult = GetConfirmationConfiguration(requestedBy.GuildId);
        if (configResult.IsFailed) {
            return configResult.ToResult();
        }

        var channel = configResult.Value.ConfirmationChannel;
        var discordMessageResult = await  _discordService.SendConfirmationMessage(channel, command.Title, command.Description, command.Fields, command.ImageUrl);

        if (discordMessageResult.IsFailed) {
            return discordMessageResult.ToResult();
        }
        
        var discordMessageId = discordMessageResult.Value;

        var repo = GetRepository<IConfirmationRepository>(requestedBy.GuildId);
        return repo.Insert(new Confirmation(discordMessageId, requestedBy.Id, null, command));
    }


    public Result Confirm(bool accepted, DiscordMessageId messageId, GuildUser confirmedBy) {
        var repo = GetRepository<IConfirmationRepository>(confirmedBy.GuildId);
        var confirmationResult = repo.GetUnconfirmedByMessageId(messageId);
        if (confirmationResult.IsFailed) {
            return confirmationResult.ToResult();
        }

        if (SendCommand(accepted, confirmationResult.Value.ConfirmCommand, confirmedBy)) {
            return null;
        }

        var confirmation = confirmationResult.Value with {ConfirmedBy = confirmedBy.Id, IsConfirmed = accepted, IsDenied = !accepted};
        return repo.Update(confirmation);
    }

    public Result SetConfirmChannel(Channel channelId, GuildUser requestedBy) {
        var repo = GetRepository<IConfirmConfigurationRepository>(requestedBy.GuildId);
        var configResult = repo.GetSingle();

        var configuration = configResult.IsFailed || configResult.Value is null
            ? new ConfirmationConfiguration(requestedBy, channelId.Id)
            : configResult.Value with { ConfirmationChannel = channelId.Id };

        return repo.UpdateOrInsert(configuration);
    }

    private bool SendCommand(bool accepted, IConfirmCommand command, GuildUser confirmedBy) {
        if (!accepted) {
            return false;
        }

        command.Handler = confirmedBy;
        var result = _mediator.Send(command);

        if (result.IsFaulted || result.IsCanceled) {
            return true;
        }

        if (result.Result.IsFailed) {
            return true;
        }

        return false;
    }
    
    private Result<ConfirmationConfiguration> GetConfirmationConfiguration(DiscordGuildId guildId) {
        var repo = GetRepository<IConfirmConfigurationRepository>(guildId);
        return repo.GetSingle();
    }
}


