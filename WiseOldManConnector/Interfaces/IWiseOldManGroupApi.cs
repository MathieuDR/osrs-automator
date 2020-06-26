using System.Collections.Generic;
using System.Threading.Tasks;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.API.Responses.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.Requests;
using WiseOldManConnector.Models.WiseOldMan.Enums;
using Group = WiseOldManConnector.Models.Output.Group;

namespace WiseOldManConnector.Interfaces {
    public interface IWiseOldManGroupApi {
        #region groups

        Task<ConnectorCollectionResponse<Group>> Search(int playerId);
        Task<ConnectorCollectionResponse<Group>> Search(string name);
        Task<ConnectorCollectionResponse<Group>> Search(string name, int limit, int offset);
        Task<ConnectorResponse<Group>> View(int groupId);
        Task<ConnectorCollectionResponse<Player>> GetMembers(int groupId);
        Task<ConnectorResponse<Player>> GetMonthTopMember(int groupId);
        Task<ConnectorResponse<Leaderboard>> GetDeltaLeaderboard(int groupId, MetricType metric, Period period);
        Task<ConnectorResponse<Leaderboard>> GetDeltaLeaderboard(int groupId, MetricType metric, Period period, int limit, int offset);
        Task<ConnectorResponse<Highscore>> GetHiscores(int groupId, MetricType metric);
        Task<ConnectorResponse<Highscore>> GetHiscores(int groupId, MetricType metric, int limit, int offset);
        Task<ConnectorResponse<Leaderboard>> GetRecordLeaderboard(int groupId, MetricType metric, Period period);
        Task<ConnectorResponse<Leaderboard>> GetRecordLeaderboard(int groupId, MetricType metric, Period period, int limit, int offset);
        Task<ConnectorCollectionResponse<Achievement>> GetRecentAchievements(int groupId);
        Task<ConnectorCollectionResponse<Achievement>> GetRecentAchievements(int groupId, int limit, int offset);
        Task<ConnectorResponse<Statistics>> GetStatistics(int groupId);
        Task<ConnectorResponse<Group>> Create(CreateGroupRequest request);
        Task<ConnectorResponse<Group>> Edit(EditGroupRequest request);
        Task<ConnectorResponse<MessageResponse>> Delete(int groupId, string verificationCode);
        Task<ConnectorResponse<Group>> AddMembers(string verificationCode, IEnumerable<GroupMember> members);
        Task<ConnectorResponse<Group>> AddMembers(string verificationCode, IEnumerable<string> members);
        Task<ConnectorResponse<Group>> RemoveMembers(string verificationCode, IEnumerable<string> members);
        Task<ConnectorResponse<Player>> ChangeMemberRole(string verificationCode, string username, GroupRole role);
        Task<ConnectorResponse<MessageResponse>> Update(string groupId);

        #endregion
    }
}