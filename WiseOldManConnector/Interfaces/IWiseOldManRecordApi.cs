using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Interfaces;

public interface IWiseOldManRecordApi {
    #region Records

    Task<ConnectorCollectionResponse<Record>> View(MetricType metric, Period period);
    Task<ConnectorCollectionResponse<Record>> View(MetricType metric, Period period, PlayerType playerType);

    #endregion
}
