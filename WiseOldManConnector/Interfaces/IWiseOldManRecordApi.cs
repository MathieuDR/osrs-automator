using System.Threading.Tasks;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Interfaces {
    public interface IWiseOldManRecordApi {
        #region Records
        Task<ConnectorCollectionResponse<Leaderboard>> View(MetricType metric); 
        Task<ConnectorCollectionResponse<Leaderboard>> View(MetricType metric, Period period); 
        Task<ConnectorCollectionResponse<Leaderboard>> View(MetricType metric, Period period, PlayerType playerType); 
        Task<ConnectorCollectionResponse<Leaderboard>> View(MetricType metric, PlayerType playerType); 
        #endregion
    }
}