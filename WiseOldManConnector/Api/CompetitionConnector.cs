using RestSharp;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.Requests;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Api;

internal class CompetitionConnector : BaseConnecter, IWiseOldManCompetitionApi {
    public CompetitionConnector(IServiceProvider provider) : base(provider) {
        Area = "competitions";
    }

    protected override string Area { get; }


    public async Task<ConnectorResponse<Competition>> View(int id) {
        var request = GetNewRestRequest("{id}");
        request.AddParameter("id", id, ParameterType.UrlSegment);

        var result = await ExecuteRequest<WOMCompetition>(request);
        return GetResponse<Competition>(result);
    }

    public async Task<ConnectorResponse<Competition>> View(int id, MetricType metric) {
        var request = GetNewRestRequest("{id}");
        request.AddParameter("id", id, ParameterType.UrlSegment);
        request.AddParameter("metric", metric.ToEnumMemberOrDefault(), ParameterType.QueryString);

        var result = await ExecuteRequest<WOMCompetition>(request);
        return GetResponse<Competition>(result);
    }

    public async Task<ConnectorResponse<Competition>> Create(CreateCompetitionRequest request) {
        var webRequest = GetNewRestRequest();
        webRequest.Method = Method.Post;
        webRequest.AddJsonBody(request);

        var result = await ExecuteRequest<WOMCompetition>(webRequest);
        return GetResponse<Competition>(result);
    }
}
