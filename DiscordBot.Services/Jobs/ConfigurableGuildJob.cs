using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Jobs; 

public abstract class ConfigurableGuildJob : BaseGuildJob {
    protected ConfigurableGuildJob(ILogger logger, IDiscordService discordService, JobType jobType, IRepositoryStrategy repositoryStrategy) :
        base(logger, discordService) {
        JobType = jobType;
        RepositoryStrategy = repositoryStrategy;
    }

    protected JobType JobType { get; }
    protected IRepositoryStrategy RepositoryStrategy { get; }

    protected override Task<Result> DoWorkForGuild(Guild guild) {
        var repo = RepositoryStrategy.GetOrCreateRepository<IGuildConfigRepository>(guild.Id);
        var guildConfiguration = repo.GetSingle().ValueOrDefault;
        ChannelJobConfiguration jobConfiguration = null;
        guildConfiguration?.AutomatedMessagesConfig?.ChannelJobs?.TryGetValue(JobType, out jobConfiguration);
        return DoWorkForGuildWithContext(guild, guildConfiguration, jobConfiguration);
    }

    protected abstract Task<Result> DoWorkForGuildWithContext(Guild guild, GuildConfig guildConfig, ChannelJobConfiguration configuration);
}