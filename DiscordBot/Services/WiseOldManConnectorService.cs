using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBotFanatic.Services.interfaces;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.Requests;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBotFanatic.Services {
    public class WiseOldManConnectorService : IOsrsHighscoreService {
        private readonly IWiseOldManPlayerApi _playerApi;
        private readonly IWiseOldManGroupApi _groupApi;
        private readonly IWiseOldManCompetitionApi _competitionApi;

        public WiseOldManConnectorService(IWiseOldManPlayerApi playerApi, IWiseOldManGroupApi groupApi, IWiseOldManCompetitionApi competitionApi) {
            _playerApi = playerApi;
            _groupApi = groupApi;
            _competitionApi = competitionApi;
        }

        public async Task<Player> GetPlayersForUsername(string username) {
            var response = await _playerApi.Search(username);
            if (response.Data.Count() == 1) {
                return response.Data.FirstOrDefault();
            }else if (response.Data.Count() > 1) {
                throw new Exception($"Too many result with the search parameter {username}");
            }

            var trackingResponse = await _playerApi.Track(username);

            return trackingResponse.Data;
        }

        public async Task<Group> GetGroupById(int womGroupId) {
            var response = await _groupApi.View(womGroupId);
            return response.Data;
        }

        public Task AddOsrsAccountToToGroup(int groupId, string verificationCode, string osrsAccount) {
            return AddOsrsAccountToToGroup(groupId, verificationCode, new[] {osrsAccount});
        }

        public Task AddOsrsAccountToToGroup(int groupId, string verificationCode, IEnumerable<string> osrsAccounts) {
            return _groupApi.AddMembers(groupId, verificationCode, osrsAccounts.Distinct().Select(s=> new MemberRequest(){Name = s, Role = GroupRole.Member}));
        }

        public async Task<Competition> GetCompetition(int competitionId) {
            return (await _competitionApi.View(competitionId)).Data;
        }

        public Task<Competition> GetCompetition(string competitionTitle) {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Competition>> GetAllCompetitionsForGroup(int groupId) {
            var response = await _groupApi.Competitions(groupId);
            return response.Data;
        }

        public async Task<DeltaLeaderboard> GetTopDeltasOfGroup(int groupId, MetricType metricType, Period period) {
            return (await _groupApi.GainedLeaderboards(groupId, metricType, period)).Data;
        }

        public async Task<IEnumerable<Achievement>> GetGroupAchievements(int groupId) {
            return (await _groupApi.RecentAchievements(groupId)).Data;
        }

        public async Task<MessageResponse> UpdateGroup(int groupId) {
            return (await _groupApi.Update(groupId)).Data;

        }
    }
}