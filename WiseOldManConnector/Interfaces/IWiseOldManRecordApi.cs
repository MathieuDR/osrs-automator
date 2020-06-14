using System.Threading.Tasks;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.API.Responses.Models;
using WiseOldManConnector.Models.WiseOldMan;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Interfaces {
    public interface IWiseOldManRecordApi {
        #region Records
        Task<ConnectorCollectionResponse<Record>> View(int playerId);
        Task<ConnectorCollectionResponse<Record>> View(int playerId, MetricType metric);
        Task<ConnectorCollectionResponse<Record>> View(int playerId, Period period);
        Task<ConnectorCollectionResponse<Record>> View(int playerId, MetricType metric, Period period);

        Task<ConnectorCollectionResponse<Leaderboard>> View(MetricType metric); 
        Task<ConnectorCollectionResponse<Leaderboard>> View(MetricType metric, Period period); 
        Task<ConnectorCollectionResponse<Leaderboard>> View(MetricType metric, Period period, PlayerType playerType); 
        Task<ConnectorCollectionResponse<Leaderboard>> View(MetricType metric, PlayerType playerType); 
        #endregion
    }
}