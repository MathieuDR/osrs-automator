using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data.Counting;
using DiscordBot.Common.Models.Data.Items;
using FluentResults;

namespace DiscordBot.Services.Interfaces;

public interface ICounterService {
    int TotalCount(GuildUser user);
    Task<Dictionary<GuildUser, int>> Count(IEnumerable<GuildUser> users, GuildUser requester, int additive, string reason);
    UserCountInfo GetCountInfo(GuildUser user);
    List<UserCountInfo> TopCounts(Guild contextGuild, int quantity);
    (List<UserCountInfo> users, int startIndex) CountRanking(GuildUser user, int quantity);
    Task<bool> SetChannelForCounts(GuildUser user, Channel outputChannel);
    Task<bool> CreateThreshold(GuildUser creator, int count, string name, Role role = null);
    Task<bool> RemoveThreshold(DiscordGuildId guildId, int index);
    Task<IReadOnlyList<CountThreshold>> GetThresholds(DiscordGuildId guildId);
    Task<DiscordChannelId> GetChannelForGuild(DiscordGuildId guildId);
    Task<IEnumerable<UserCountInfo>> GetAllUserInfo(Guild toGuildDto);
    Task<List<Item>> GetItemsForGuild(Guild toGuildDto);
    Task<IEnumerable<(string synonym, Item item)>> GetItemsForGuild(Guild toGuildDto, string autocomplete);
    Task<Result<bool>> CanSelfCountInChannel(GuildUser user, Channel channel);
    Task<Result> SelfCount(GuildUser user, Item item, GuildUser[] splits, String imageUrl);
    
    Task<Result> SetRequestChannel(GuildUser user, Channel channel);
    Task<Result> SaveItemsFromJson(GuildUser user, string json);
}
