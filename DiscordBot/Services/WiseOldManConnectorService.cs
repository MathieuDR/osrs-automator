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

        public Task AddOsrsAccountToToGroup(in int groupId, in string verificationCode, in string osrsAccount) {
            return AddOsrsAccountToToGroup(groupId, verificationCode, new[] {osrsAccount});
        }

        public Task AddOsrsAccountToToGroup(in int groupId, in string verificationCode, in IEnumerable<string> osrsAccounts) {
            return _groupApi.AddMembers(groupId, verificationCode, osrsAccounts.Select(s=> new MemberRequest(){Name = s, Role = GroupRole.Member}));
        }
    }
}