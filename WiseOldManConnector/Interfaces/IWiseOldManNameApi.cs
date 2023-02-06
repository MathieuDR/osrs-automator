using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Interfaces;

public interface IWiseOldManNameApi {
    Task<ConnectorResponse<NameChange>> Request(string oldUsername, string newUsername);

}
