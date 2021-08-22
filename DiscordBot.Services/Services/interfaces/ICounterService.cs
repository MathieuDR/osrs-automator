using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.DiscordDtos;

namespace DiscordBot.Services.Services.interfaces {
    public interface ICounterService {
        int TotalCount(GuildUser user);
        int Count(GuildUser user, GuildUser requester, int additive, string reason);
        UserCountInfo GetCountInfo(GuildUser user);
        List<UserCountInfo> TopCounts(Guild contextGuild, int quantity);
        
        Task<bool> SetChannelForCounts(GuildUser creator, Channel outputChannel);
        Task<bool> CreateTreshold(GuildUser creator, int count, string name, Role role = null);        
        Task<bool> RemoveCount(ulong guildId, int index);
        Task<IReadOnlyList<CountThreshold>> GetTresholds(ulong guildId);

        Task<ulong> GetChannelForGuild(ulong guildId);
    }
}
