using RestSharp;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Api;

internal class NameConnector : BaseConnecter, IWiseOldManNameApi {
    public NameConnector(IServiceProvider provider) : base(provider) { }
    protected override string Area { get; } = "names";

   public async Task<ConnectorResponse<NameChange>> Request(string oldUsername, string newUsername) {
        var request = GetNewRestRequest();
        request.Method = Method.Post;
        request.AddJsonBody(new {
            oldName = oldUsername,
            newName = newUsername
        });

        var result = await ExecuteRequest<NameChangeResponse>(request);
        return GetResponse<NameChange>(result);
    }
}
