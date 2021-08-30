using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.DiscordDtos;
using FluentResults;
using WiseOldManConnector.Models.Output;

namespace DiscordBot.Services.Interfaces {
    public interface IDiscordService {
        Task<Result> SetUsername(GuildUser user, string nickname);
        Task<Result> PrintRunescapeDataDrop(RunescapeDropData data, ulong guildId, ulong channelId);
        Task<Result<IEnumerable<Guild>>> GetAllGuilds();
        Task<Result> SendFailedEmbed(ulong channelId, string message, Guid traceId);
        Task<Result> SendWomGroupSuccessEmbed(ulong channelId, string message, int groupId, string groupName);
        Task<Result> MessageLeaderboards(ulong channelId, IEnumerable<Leaderboard> leaderboards);
        Task<Result> MessageLeaderboards<T>(ulong channelId, IEnumerable<MetricTypeLeaderboard<T>> leaderboards);
        Task<Result> MessageLeaderboards<T>(ulong channelId, IEnumerable<MetricTypeAndPeriodLeaderboard<T>> leaderboards);
        
        Task<Result> MessageLeaderboard(ulong channelId, Leaderboard leaderboard);
        Task<Result> MessageLeaderboard<T>(ulong channelId, MetricTypeLeaderboard<T> leaderboard);
        Task<Result> MessageLeaderboard<T>(ulong channelId, MetricTypeAndPeriodLeaderboard<T> leaderboard);
    }
}
