using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Services.Interfaces;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.Requests;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Services.Services {
    public class WiseOldManConnectorService : IOsrsHighscoreService {
        private readonly IWiseOldManCompetitionApi _competitionApi;
        private readonly IWiseOldManGroupApi _groupApi;
        private readonly IWiseOldManPlayerApi _playerApi;
        private readonly IWiseOldManNameApi _nameApi;
        
        public WiseOldManConnectorService(IWiseOldManPlayerApi playerApi, IWiseOldManGroupApi groupApi,
            IWiseOldManCompetitionApi competitionApi, IWiseOldManNameApi nameApi) {
            _playerApi = playerApi;
            _groupApi = groupApi;
            _competitionApi = competitionApi;
            _nameApi = nameApi;
        }

        public async Task<Player> GetPlayersForUsername(string username) {
            var response = await _playerApi.Search(username);

            var queried = response.Data.Where(x => x.Username.ToLowerInvariant() == username.ToLowerInvariant()).ToList();
            
            if (queried.Count() == 1) {
                return queried.FirstOrDefault();
            }

            if (response.Data.Count() > 1) {
                throw new Exception($"Too many result with the search parameter {username}, but none specific.");
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
            return _groupApi.AddMembers(groupId, verificationCode,
                osrsAccounts.Distinct().Select(s => new MemberRequest() {Name = s, Role = GroupRole.Member}));
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

        public async Task<HighscoreLeaderboard> GetLeaderboard(int groupId, MetricType metricType) {
            return (await _groupApi.Highscores(groupId, metricType)).Data;
        }

        public async Task<IEnumerable<Achievement>> GetGroupAchievements(int groupId) {
            return (await _groupApi.RecentAchievements(groupId)).Data;
        }

        public async Task<MessageResponse> UpdateGroup(int groupId, string verificationCode) {
            return (await _groupApi.Update(groupId, verificationCode)).Data;
        }

        public async Task<Player> GetPlayerById(int playerId) {
            var response = (await _playerApi.View(playerId));
            return response.Data;
        }

        public async Task<NameChange> RequestNameChange(string oldUsername, string requestedUsername) {
            return (await _nameApi.Request(oldUsername, requestedUsername)).Data;
        }
    }
}