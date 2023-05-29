using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;

namespace WiseOldManConnector.Interfaces;

public interface IWiseOldManNameApi {
    Task<ConnectorResponse<NameChange>> Request(string oldUsername, string newUsername);

}
