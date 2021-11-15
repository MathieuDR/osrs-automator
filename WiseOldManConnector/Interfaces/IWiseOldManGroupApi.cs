using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.Requests;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Interfaces;

public interface IWiseOldManGroupApi {
    #region groups

    Task<ConnectorCollectionResponse<Group>> Search();
    Task<ConnectorCollectionResponse<Group>> Search(string name);
    Task<ConnectorCollectionResponse<Group>> Search(string name, int limit, int offset);
    Task<ConnectorResponse<Group>> View(int id);
    Task<ConnectorCollectionResponse<Player>> GetMembers(int id);
    Task<ConnectorResponse<DeltaMember>> GetMonthTopMember(int id);

    Task<ConnectorCollectionResponse<Competition>> Competitions(int id);
    Task<ConnectorResponse<DeltaLeaderboard>> GainedLeaderboards(int id, MetricType metric, Period period);
    Task<ConnectorResponse<DeltaLeaderboard>> GainedLeaderboards(int id, MetricType metric, Period period, int limit, int offset);
    Task<ConnectorResponse<HighscoreLeaderboard>> Highscores(int id, MetricType metric);
    Task<ConnectorResponse<HighscoreLeaderboard>> Highscores(int id, MetricType metric, int limit, int offset);
    Task<ConnectorResponse<RecordLeaderboard>> RecordLeaderboards(int id, MetricType metric, Period period);
    Task<ConnectorResponse<RecordLeaderboard>> RecordLeaderboards(int id, MetricType metric, Period period, int limit, int offset);
    Task<ConnectorCollectionResponse<Achievement>> RecentAchievements(int id);
    Task<ConnectorCollectionResponse<Achievement>> RecentAchievements(int id, int limit, int offset);
    Task<ConnectorResponse<Statistics>> Statistics(int id);
    Task<ConnectorResponse<VerificationGroup>> Create(CreateGroupRequest request);
    Task<ConnectorResponse<Group>> Edit(int id, EditGroupRequest request);

    Task<ConnectorResponse<MessageResponse>> Delete(int id, string verificationCode);

    //Task<ConnectorResponse<WOMGroup>> AddMembers(string verificationCode, IEnumerable<GroupMember> members);
    Task<ConnectorResponse<Group>> AddMembers(int id, string verificationCode, IEnumerable<MemberRequest> members);
    Task<ConnectorResponse<Group>> RemoveMembers(int id, string verificationCode, IEnumerable<string> members);
    Task<ConnectorResponse<Player>> ChangeMemberRole(int id, string verificationCode, string username, GroupRole role);
    Task<ConnectorResponse<MessageResponse>> Update(int id, string verificationCode);

    #endregion
}
