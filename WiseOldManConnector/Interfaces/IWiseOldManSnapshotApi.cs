using System.Threading.Tasks;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Interfaces {
    public interface IWiseOldManSnapshotApi {
        #region Snapschots
        Task<ConnectorCollectionResponse<Snapshot>> View(int playerId);
        Task<ConnectorCollectionResponse<Snapshot>> View(int playerId, Period period);
        #endregion
    }
}