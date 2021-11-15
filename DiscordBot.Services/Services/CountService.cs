using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Services;

internal class CountService : RepositoryService, ICounterService {
    public CountService(ILogger<CountService> logger, IRepositoryStrategy repositoryStrategy) : base(logger, repositoryStrategy) { }


    public int TotalCount(GuildUser user) {
        var count = GetOrCreateUserCountInfo(user);
        return count.CurrentCount;
    }

    public int Count(GuildUser user, GuildUser requester, int additive, string reason) {
        var count = GetOrCreateUserCountInfo(user, requester);
        var newCount = new Count(requester.Id, requester.Username, additive, reason);
        count.CountHistory.Add(newCount);

        var repo = GetRepository<IUserCountInfoRepository>(user.GuildId);
        repo.UpdateOrInsert(count);

        return count.CurrentCount;
    }

    public UserCountInfo GetCountInfo(GuildUser user) {
        return GetOrCreateUserCountInfo(user);
    }

    public List<UserCountInfo> TopCounts(Guild guild, int quantity) {
        var repo = GetRepository<IUserCountInfoRepository>(guild.Id);
        var all = repo.GetAll().Value;
        return all.OrderByDescending(c => c.CurrentCount).Take(quantity).ToList();
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

        var result = config.CountConfig.AddTreshold(toAdd);

        if (result) {
            var repo = GetRepository<IGuildConfigRepository>(creator.GuildId);
            repo.UpdateOrInsert(config);
        }

        return Task.FromResult(result);
    }

    public Task<bool> RemoveThreshold(ulong guildId, int index) {
        var config = GetGroupConfigWithValidCountConfig(guildId);
        var result = config.CountConfig.RemoveAtIndex(index);

        if (result) {
            var repo = GetRepository<IGuildConfigRepository>(guildId);
            repo.UpdateOrInsert(config);
        }

        return Task.FromResult(result);
    }

    public Task<IReadOnlyList<CountThreshold>> GetThresholds(ulong guildId) {
        var config = GetGroupConfigWithValidCountConfig(guildId);
        return Task.FromResult(config.CountConfig.Tresholds);
    }

    public Task<ulong> GetChannelForGuild(ulong guildId) {
        var config = GetGroupConfigWithValidCountConfig(guildId);
        return Task.FromResult(config.CountConfig.OutputChannelId);
    }


    private UserCountInfo GetOrCreateUserCountInfo(GuildUser user, GuildUser requester = null) {
        var repo = GetRepository<IUserCountInfoRepository>(user.GuildId);
        var result = repo.GetByDiscordUserId(user.Id).Value ?? new UserCountInfo(requester?.Id ?? user.Id) { DiscordId = user.Id };

        return result;
    }

    private GuildConfig GetGroupConfigWithValidCountConfig(ulong guildId) {
        var config = GetGroupConfig(guildId);

        if (config.CountConfig is null) {
            throw new Exception("Please set up an output channel first.");
        }

        return config;
    }

    private GuildConfig GetGroupConfig(ulong guildId, bool validate = true) {
        var repo = GetRepository<IGuildConfigRepository>(guildId);
        var result = repo.GetSingle().Value;

        if (result == null) {
            throw new Exception("Guild has no configuration. Please set the config");
        }

        return result;
    }
}
