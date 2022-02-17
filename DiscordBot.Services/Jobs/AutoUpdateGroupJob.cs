using System.Text;
using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Data.Configuration;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;
using WiseOldManConnector.Models.Output.Exceptions;

namespace DiscordBot.Services.Jobs;

public class AutoUpdateGroupJob : ConfigurableGuildJob {
    private readonly IOsrsHighscoreService _osrsHighscoreService;

    public AutoUpdateGroupJob(ILogger<AutoUpdateGroupJob> logger, IDiscordService discordService, IRepositoryStrategy repositoryStrategy,
        IOsrsHighscoreService osrsHighscoreService) : base(logger, discordService, JobType.GroupUpdate, repositoryStrategy) {
        _osrsHighscoreService = osrsHighscoreService;
    }

    protected override async Task<Result> DoWorkForGuildWithContext(Guild guild, GuildConfig guildConfig, ChannelJobConfiguration configuration) {
        if (configuration is null || !configuration.IsEnabled) {
            return Result.Ok();
        }

        var response = new StringBuilder();
        response.AppendLine("Updated wise old man!");

        try {
            var message = await _osrsHighscoreService.UpdateGroup(guildConfig.WomGroupId, guildConfig.WomVerificationCode);
            response.AppendLine(message.Message);
        } catch (BadRequestException badRequestException) {
            var errorGuid = Guid.NewGuid();
            Logger.LogError(badRequestException, "Cannot update group - {trace}", errorGuid);
            await DiscordService.SendFailedEmbed(configuration.ChannelId, "Could not update group", errorGuid);
            return Result.Fail(new ExceptionalError("Could not update WOM", badRequestException));
        }

        return await DiscordService.SendWomGroupSuccessEmbed(configuration.ChannelId, response.ToString(), guildConfig.WomGroup.Id,
            guildConfig.WomGroup.Name);
    }
}
