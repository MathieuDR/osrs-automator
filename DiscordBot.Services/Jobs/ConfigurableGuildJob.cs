using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data.Configuration;
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
        // This method is synchronous (returns a Task without awaiting it), so the lease held by
        // `repo` is released as soon as this method returns - i.e. before DoWorkForGuildWithContext's
        // work actually runs. That's fine only because guildConfiguration/jobConfiguration below are
        // plain materialised values (not lazily backed by the repository/lease), so nothing handed
        // down to the concrete job needs the database to still be open.
        using var repo = RepositoryStrategy.GetOrCreateRepository<IGuildConfigRepository>(guild.Id);
        var guildConfiguration = repo.GetSingle().ValueOrDefault;
        ChannelJobConfiguration jobConfiguration = null;
        guildConfiguration?.AutomatedMessagesConfig?.ChannelJobs?.TryGetValue(JobType, out jobConfiguration);

        if (jobConfiguration is not null) {
            return DoWorkForGuildWithContext(guild, guildConfiguration, jobConfiguration);    
        }
        
        return Task.FromResult(Result.Ok());
    }

    protected abstract Task<Result> DoWorkForGuildWithContext(Guild guild, GuildConfig guildConfig, ChannelJobConfiguration configuration);
}
