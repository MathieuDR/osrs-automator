using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Interfaces; 

public interface IWiseOldManNameApi {
    Task<ConnectorResponse<NameChange>> Request(string oldUsername, string newUsername);

    /// <summary>
    ///     Change multiple usernames
    /// </summary>
    /// <param name="items">Tuple with oldname - newname</param>
    /// <returns>NameChange</returns>
    Task<ConnectorCollectionResponse<NameChange>> Request(IEnumerable<Tuple<string, string>> items);

    #region list

    Task<ConnectorCollectionResponse<NameChange>> View(int limit = 20, int offset = 0);
    Task<ConnectorCollectionResponse<NameChange>> View(string username, int limit = 20, int offset = 0);
    Task<ConnectorCollectionResponse<NameChange>> View(NameChangeStatus status, int limit = 20, int offset = 0);

    Task<ConnectorCollectionResponse<NameChange>> View(string username, NameChangeStatus status, int limit = 20, int offset = 0);

    #endregion
}