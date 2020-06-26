using System.Threading.Tasks;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Interfaces {
    public interface IWiseOldManDeltaApi {
        #region Deltas
  
        Task<ConnectorCollectionResponse<Delta>> ViewLeaderboard(MetricType metric);
        Task<ConnectorCollectionResponse<Delta>> ViewLeaderboard(MetricType metric, Period period);
        Task<ConnectorCollectionResponse<Delta>> ViewLeaderboard(MetricType metric, PlayerType playertype);
        Task<ConnectorCollectionResponse<Delta>> ViewLeaderboard(MetricType metric, Period period, PlayerType playertype);

        #endregion
    }
}