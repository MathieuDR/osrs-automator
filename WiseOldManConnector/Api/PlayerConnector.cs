using RestSharp;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Api;

internal class PlayerConnector : BaseConnecter, IWiseOldManPlayerApi {
    public PlayerConnector(IServiceProvider provider) : base(provider) {
        Area = "players";
    }

    protected override string Area { get; }

    #region search

    public async Task<ConnectorCollectionResponse<Player>> Search(string username) {
        var request = GetNewRestRequest("search");
        request.AddParameter("username", username);

        var result = await ExecuteCollectionRequest<PlayerResponse>(request);
        return GetResponse<PlayerResponse, Player>(result);
    }

    #endregion

    #region track

    public async Task<ConnectorResponse<Player>> Track(string username) {
        var request = GetNewRestRequest("track");
        request.Method = Method.Post;
        username = username.ToLowerInvariant();
        request.AddJsonBody(new { username });

        var result = await ExecuteRequest<PlayerResponse>(request);
        return GetResponse<Player>(result);
    }

    #endregion

 


    #region view

    public async Task<ConnectorResponse<Player>> View(int id) {
        var request = GetNewRestRequest("id/{id}");
        request.AddParameter("id", id, ParameterType.UrlSegment);

        var result = await ExecuteRequest<PlayerResponse>(request);
        return GetResponse<Player>(result);
    }

    #endregion
}
