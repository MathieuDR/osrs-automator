using System.Collections.Generic;
using System.Threading.Tasks;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBotFanatic.Services.interfaces {
    public interface IOsrsHighscoreService {
        Task<Player> GetPlayersForUsername(string username);
        Task<Group> GetGroupById(int womGroupId);
        Task AddOsrsAccountToToGroup(int groupId, string verificationCode, string osrsAccount);
        Task AddOsrsAccountToToGroup(int groupId, string verificationCode, IEnumerable<string> osrsAccounts);
        Task<Competition> GetCompetition(int competitionId);
        Task<Competition> GetCompetition(string competitionTitle);
        Task<IEnumerable<Competition>> GetAllCompetitionsForGroup(int groupId);
        Task<DeltaLeaderboard> GetTopDeltasOfGroup(int groupId, MetricType metricType, Period period);
        Task<IEnumerable<Achievement>> GetGroupAchievements(int groupId);
    }
}