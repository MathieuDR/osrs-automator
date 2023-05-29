using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Helpers.Extensions;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data.Configuration;
using DiscordBot.Common.Models.Data.Counting;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Interfaces;
using FluentResults;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Services;

internal class CountService : RepositoryService, ICounterService {
    private readonly IDiscordService _discordService;

    public CountService(ILogger<CountService> logger, IRepositoryStrategy repositoryStrategy, IDiscordService discordService) : base(logger,
        repositoryStrategy) => _discordService = discordService;


    public int TotalCount(GuildUser user) {
        var count = GetOrCreateUserCountInfo(user);
        return count.CurrentCount;
    }

    public async Task<Dictionary<GuildUser, int>> Count(IEnumerable<GuildUser> users, GuildUser requester, int additive, string reason) {
        var config = GetGroupConfigWithValidCountConfig(requester.GuildId);

        var (dict, counts) = CreateCountHistory(users, requester, additive, reason);
        UpsertCounts(requester, counts);

        var (roleDict, messages) = TriggerThreDictionary(additive, config, dict);

        var messageTask = SendMessage(messages, config);
        await HandleRoles(requester, additive, roleDict);
        await messageTask;
        
        return dict.ToDictionary(x => x.Key, x => x.Value.newCount);
    }

    private async Task HandleRoles(GuildUser requester, int additive, Dictionary<DiscordRoleId, List<DiscordUserId>> roleDict) {
        var dictParam = roleDict
            .ToDictionary(x => x.Key, x => x.Value.AsEnumerable())
            .ReverseDictionary();

        if (additive > 0) {
            await _discordService.AddRoles(requester.GuildId, dictParam);
        } else {
            await _discordService.RemoveRoles(requester.GuildId, dictParam);
        }
    }

    private Task<Result> SendMessage(List<string> messages, GuildConfig config) {
        if (!messages.Any()) {
            return Task.FromResult(Result.Ok());
        }
        var message = string.Join(Environment.NewLine, messages);
        var messageTask = _discordService.SendSuccessEmbed(config.CountConfig.OutputChannelId, message);
        return messageTask;
    }

    private static (Dictionary<DiscordRoleId, List<DiscordUserId>> UserPerRolesDictionary, List<string> Messages) TriggerThreDictionary(int additive, GuildConfig config, Dictionary<GuildUser, (int oldCount, int newCount)> dict) {
        var thresholds = config.CountConfig.Thresholds.OrderBy(x => x.Threshold).ToList();
        var roleDict = new Dictionary<DiscordRoleId, List<DiscordUserId>>();
        var isPositive = additive > 0;
        var messages = new List<string>();

        foreach (var (user, countValue) in dict) {
            foreach (var threshold in thresholds) {
                var value = threshold.Threshold;


                if (isPositive && countValue.oldCount < value && countValue.newCount >= value) {
                    // Hit it
                    messages.Add($"{threshold.Name} hit for <@{user.Id}>!");

                    if (threshold.GivenRoleId.HasValue) {
                        roleDict.Insert(threshold.GivenRoleId.Value, user.Id);
                    }

                    continue;
                }

                if (isPositive && countValue.newCount < value) {
                    break;
                }

                if (!isPositive && countValue.oldCount >= value && countValue.newCount < value) {
                    // Remove it
                    messages.Add($"<@{user.Id}> has not sufficient points anymore for {threshold.Name}");

                    if (threshold.GivenRoleId.HasValue) {
                        roleDict.Insert(threshold.GivenRoleId.Value, user.Id);
                    }

                    continue;
                }

                if (!isPositive && countValue.newCount >= value) {
                    break;
                }
            }
        }

        return (roleDict, messages);
    }

    private void UpsertCounts(GuildUser requester, List<UserCountInfo> counts) {
        var repo = GetRepository<IUserCountInfoRepository>(requester.GuildId);
        repo.BulkUpdateOrInsert(counts);
    }

    private (Dictionary<GuildUser, (int oldCount, int newCount)> UserCountValuesDictionary, List<UserCountInfo> CountObjects) CreateCountHistory(IEnumerable<GuildUser> users, GuildUser requester, int additive,
        string reason) {
        var dict = new Dictionary<GuildUser, (int oldCount, int newCount)>();
        var counts = new List<UserCountInfo>();

        foreach (var user in users.Distinct()) {
            var count = GetOrCreateUserCountInfo(user, requester);
            var newCount = new Count(requester.Id, requester.Username, additive, reason);
            var currentCount = count.CurrentCount;
            count.CountHistory.Add(newCount);
            dict.Add(user, (currentCount, currentCount + additive));
            counts.Add(count);
        }

        return (dict, counts);
    }

    public UserCountInfo GetCountInfo(GuildUser user) => GetOrCreateUserCountInfo(user);

    public List<UserCountInfo> TopCounts(Guild guild, int quantity) {
        var repo = GetRepository<IUserCountInfoRepository>(guild.Id);
        var all = repo.GetAll().Value;
        return all.OrderByDescending(c => c.CurrentCount).Take(quantity).ToList();
    }

    public (List<UserCountInfo> users, int startIndex) CountRanking(GuildUser user, int quantity) {
        var repo = GetRepository<IUserCountInfoRepository>(user.GuildId);
        var all = repo.GetAll().Value.OrderByDescending(c => c.CurrentCount).ToList();
        var userCountInfo = all.FirstOrDefault(x => x.DiscordId == user.Id);

        var userPos = 0;
        var before = quantity / 2;
        if (userCountInfo is not null) {
            userPos = all.IndexOf(userCountInfo);
        }

        var leftBound = Math.Max(0, userPos - before);
        return (all.Skip(leftBound).Take(quantity).ToList(), leftBound);
    }

    public Task<bool> SetChannelForCounts(GuildUser user, Channel outputChannel) {
        var repo = GetRepository<IGuildConfigRepository>(user.GuildId);
        var config = GetGroupConfig(user.GuildId);
        config.CountConfig ??= new CountConfig();

        config.CountConfig.OutputChannelId = outputChannel.Id;
        repo.UpdateOrInsert(config);
        return Task.FromResult(true);
    }

    public Task<bool> CreateThreshold(GuildUser creator, int count, string name, Role role = null) {
        var config = GetGroupConfigWithValidCountConfig(creator.GuildId);
        var toAdd = new CountThreshold();
        toAdd.Name = string.IsNullOrEmpty(name) ? "unnamed" : name;
        toAdd.Threshold = count;
        toAdd.GivenRoleId = role?.Id;
        toAdd.CreatorId = creator.Id;
        toAdd.CreatorUsername = creator.Username;

        var result = config.CountConfig.AddThreshold(toAdd);

        if (result) {
            var repo = GetRepository<IGuildConfigRepository>(creator.GuildId);
            repo.UpdateOrInsert(config);
        }

        return Task.FromResult(result);
    }

    public Task<bool> RemoveThreshold(DiscordGuildId guildId, int index) {
        var config = GetGroupConfigWithValidCountConfig(guildId);
        var result = config.CountConfig.RemoveAtIndex(index);

        if (result) {
            var repo = GetRepository<IGuildConfigRepository>(guildId);
            repo.UpdateOrInsert(config);
        }

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<CountThreshold>> GetThresholds(DiscordGuildId guildId) {
        var config = GetGroupConfigWithValidCountConfig(guildId);
        return Task.FromResult(config.CountConfig.Thresholds);
    }

    public Task<DiscordChannelId> GetChannelForGuild(DiscordGuildId guildId) {
        var config = GetGroupConfigWithValidCountConfig(guildId);
        return Task.FromResult(config.CountConfig.OutputChannelId);
    }

    public Task<IEnumerable<UserCountInfo>> GetAllUserInfo(Guild guild) {
        
        var repo = GetRepository<IUserCountInfoRepository>(guild.Id);
        var allResult = repo.GetAll();

        if (allResult.IsFailed) {
            return Task.FromResult(Array.Empty<UserCountInfo>().AsEnumerable());
        }

       

        return Task.FromResult(allResult.Value);
    }

    private UserCountInfo GetOrCreateUserCountInfo(GuildUser user, GuildUser requester = null) {
        var repo = GetRepository<IUserCountInfoRepository>(user.GuildId);
        var result = repo.GetByDiscordUserId(user.Id).Value ?? new UserCountInfo(requester?.Id ?? user.Id) { DiscordId = user.Id };

        return result;
    }

    private GuildConfig GetGroupConfigWithValidCountConfig(DiscordGuildId guildId) {
        var config = GetGroupConfig(guildId);

        if (config.CountConfig is null) {
            throw new Exception("Please set up an output channel first.");
        }

        return config;
    }

    private GuildConfig GetGroupConfig(DiscordGuildId guildId, bool validate = true) {
        var repo = GetRepository<IGuildConfigRepository>(guildId);
        var result = repo.GetSingle().Value;

        if (result == null) {
            throw new Exception("Guild has no configuration. Please set the config");
        }

        return result;
    }
}
