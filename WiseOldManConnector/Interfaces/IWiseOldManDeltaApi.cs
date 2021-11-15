using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Interfaces;

public interface IWiseOldManDeltaApi {
    #region Deltas

    Task<ConnectorCollectionResponse<Deltas>> ViewLeaderboard(MetricType metric);
    Task<ConnectorCollectionResponse<Deltas>> ViewLeaderboard(MetricType metric, Period period);
    Task<ConnectorCollectionResponse<Deltas>> ViewLeaderboard(MetricType metric, PlayerType playertype);
    Task<ConnectorCollectionResponse<Deltas>> ViewLeaderboard(MetricType metric, Period period, PlayerType playertype);

    #endregion
}
