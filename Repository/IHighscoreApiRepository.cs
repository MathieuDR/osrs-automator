using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses;
using RestSharp;

namespace DiscordBotFanatic.Repository {
    public interface IHighscoreApiRepository {
        public Task<IEnumerable<SearchResponse>> SearchPlayerAsync(string username);
        public Task<PlayerResponse> GetPlayerAsync(string username);
        public Task<PlayerResponse> TrackPlayerAsync(string username);
        public Task<DeltaResponse> DeltaPlayerAsync(int id, Period period = Period.Week, MetricType? metric = null);
        public Task<PlayerResponse> GetPlayerAsync(int id);
        public Task<RecordResponse> GetPlayerRecordAsync(int id, MetricType? metric, Period? period);
        public Task<GroupUpdateResponse> UpdateGroupAsync(int groupId);
        public Task<GroupMembersListResponse> GetPlayersFromGroupId(int groupId);
    }
}