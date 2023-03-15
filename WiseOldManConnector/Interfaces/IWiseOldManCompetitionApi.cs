using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.Requests;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Interfaces;

public interface IWiseOldManCompetitionApi {
    Task<ConnectorResponse<Competition>> View(int id);
    Task<ConnectorResponse<Competition>> View(int id, MetricType metric);

    Task<ConnectorResponse<Competition>> Create(CreateCompetitionRequest request);
}
