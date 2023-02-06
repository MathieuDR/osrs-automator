using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.Requests;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Interfaces;

public interface IWiseOldManGroupApi {
    #region groups
    Task<ConnectorResponse<Group>> View(int id);
    Task<ConnectorCollectionResponse<Player>> GetMembers(int id);
    Task<ConnectorCollectionResponse<Competition>> Competitions(int id);
    Task<ConnectorResponse<DeltaLeaderboard>> GainedLeaderboards(int id, MetricType metric, Period period);
    Task<ConnectorResponse<HighscoreLeaderboard>> Highscores(int id, MetricType metric);
    Task<ConnectorCollectionResponse<Achievement>> RecentAchievements(int id);
    Task<ConnectorResponse<Group>> AddMembers(int id, string verificationCode, IEnumerable<MemberRequest> members);

    Task<ConnectorResponse<MessageResponse>> Update(int id, string verificationCode);

    #endregion
}
