using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Cleaned;
using DiscordBotFanatic.Models.WiseOldMan.Responses;

namespace DiscordBotFanatic.Services.interfaces {
    public interface IOsrsHighscoreService {
        string GetUserNameFromUser(IUser user);
        Task<List<LeaderboardMemberInfo>> GetPlayerRecordsForGroupAsync(MetricType metricType, Period period, int groupId);
        Task<List<CompetitionInfo>> GetGuildCompetitionLeaderboard(IGuild guild);

        void SetDefaultPlayer(ulong userId, string username);
        Task<IEnumerable<SearchResponse>> SearchPlayerAsync(string username);
        Task<PlayerResponse> GetPlayerAsync(string username);
        Task<PlayerResponse> TrackPlayerAsync(string username);
        Task<DeltaResponse> DeltaPlayerAsync(int id, Period period = Period.Week, MetricType? metricType = null);
        Task<PlayerResponse> GetPlayerAsync(int id);
        Task<RecordResponse> GetPlayerRecordAsync(int id, MetricType? metric, Period? periodS);
        Task<GroupUpdateResponse> UpdateGroupAsync(int id);
        int GetGroupIdFromName(string name);
        
    }
}