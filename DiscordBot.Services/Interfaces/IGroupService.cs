using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Decorators;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Services.Models.Enums;
using FluentResults;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Services.Interfaces;

public interface IGroupService {
    public Task<ItemDecorator<Group>> SetGroupForGuild(GuildUser guildUser, int womGroupId, string verificationCode);
    Task SetAutoAdd(GuildUser guildUser, bool autoAdd);
    Task SetAutomationJobChannel(JobType jobType, GuildUser user, Channel messageChannel);
    Task<bool> ToggleAutomationJob(JobType jobType, Guild guild);
    Task SetActivationAutomationJob(JobType jobType, bool activated);
    Task<Dictionary<string, string>> GetSettingsDictionary(Guild guild);
    Task<ItemDecorator<Leaderboard>> GetGroupLeaderboard(GuildUser guildUser);
    Task<ItemDecorator<Leaderboard>> GetGroupLeaderboard(GuildUser guildUser, MetricType metric, Period period);
    Task QueueJob(JobType jobType);

    Task<Result<ItemDecorator<Competition>>> CreateCompetition(Guild guild, DateTimeOffset start, DateTimeOffset end, MetricType metric,
        CompetitionType competitionType, string name = null);

    Task<Result<CommandRoleConfig>> GetCommandRoleConfig(Guild guild);
}
