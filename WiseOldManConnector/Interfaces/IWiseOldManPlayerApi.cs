using System.Threading.Tasks;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.WiseOldMan;

namespace WiseOldManConnector.Interfaces {
    public interface IWiseOldManPlayerApi {
        #region Player
        Task<ConnectorCollectionResponse<Player>> Search(string username);
        Task<ConnectorResponse<Player>> View(string username);
        Task<ConnectorResponse<Player>> View(int id);
        Task<ConnectorResponse<Player>> Track(string username);
        Task<ConnectorResponse<MessageResponse>> AssertPlayerType(string username);
        Task<ConnectorResponse<MessageResponse>> AssertDisplayName(string username);
        Task<ConnectorResponse<MessageResponse>> Import(string username);
        #endregion

    }
}