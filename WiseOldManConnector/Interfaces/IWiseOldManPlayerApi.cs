using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Interfaces;

public interface IWiseOldManPlayerApi {
    #region search

    Task<ConnectorCollectionResponse<Player>> Search(string username);

    #endregion

    #region track

    Task<ConnectorResponse<Player>> Track(string username);

    #endregion


    #region view

    Task<ConnectorResponse<Player>> View(int id);

    #endregion

}
