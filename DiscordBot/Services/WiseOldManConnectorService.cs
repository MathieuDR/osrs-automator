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

        public WiseOldManConnectorService(IWiseOldManPlayerApi playerApi, IWiseOldManGroupApi groupApi) {
            _playerApi = playerApi;
            _groupApi = groupApi;
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

        public Task<Competition> GetCompetition(int competitionId) {
            throw new NotImplementedException();
        }

        public Task<Competition> GetCompetition(string competitionTitle) {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Competition>> GetAllCompetitionsForGroup(int groupId) {
            var response = await _groupApi.Competitions(groupId);
            return response.Data;
        }
    }
}