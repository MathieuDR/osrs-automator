using System.Collections.Generic;
using System.Threading.Tasks;
using WiseOldManConnector.Models.Output;

namespace DiscordBotFanatic.Services.interfaces {
    public interface IOsrsHighscoreService {
        Task<Player> GetPlayersForUsername(string username);
        Task<Group> GetGroupById(int womGroupId);
        Task AddOsrsAccountToToGroup(in int groupId, in string verificationCode, in string osrsAccount);
        Task AddOsrsAccountToToGroup(in int groupId, in string verificationCode, in IEnumerable<string> osrsAccounts);
    }
}