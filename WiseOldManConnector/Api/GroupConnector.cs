using RestSharp;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.Requests;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Api;

internal class GroupConnector : BaseConnecter, IWiseOldManGroupApi {
    public GroupConnector(IServiceProvider provider) : base(provider) {
        Area = "groups";
    }

    protected override string Area { get; }


    public async Task<ConnectorResponse<Group>> View(int id) {
        var request = GetNewRestRequest("{id}");

        request.AddParameter("id", id, ParameterType.UrlSegment);

        var result = await ExecuteRequest<WOMGroup>(request);
        return GetResponse<Group>(result);
    }

    public async Task<ConnectorCollectionResponse<Player>> GetMembers(int id) {
        var request = GetNewRestRequest("{id}/members");

        request.AddParameter("id", id, ParameterType.UrlSegment);

        var result = await ExecuteCollectionRequest<PlayerResponse>(request);
        return GetResponse<PlayerResponse, Player>(result);
    }

   
    public async Task<ConnectorCollectionResponse<Competition>> Competitions(int id) {
        var request = GetNewRestRequest("{id}/competitions");

        request.AddParameter("id", id, ParameterType.UrlSegment);

        var result = await ExecuteCollectionRequest<WOMCompetition>(request);
        return GetResponse<WOMCompetition, Competition>(result);
    }

    public async Task<ConnectorResponse<DeltaLeaderboard>> GainedLeaderboards(int id, MetricType metric, Period period) {
        var request = GetNewRestRequest("{id}/gained");

        request.AddParameter("id", id, ParameterType.UrlSegment);
        request.AddParameter("metric", metric.ToEnumMemberOrDefault());
        request.AddParameter("period", period.ToEnumMemberOrDefault());

        var requestResult = await ExecuteCollectionRequest<WOMGroupDeltaMember>(request);
        var result = GetResponse<DeltaLeaderboard>(requestResult);
        result.Data.PageSize = 20;
        result.Data.Page = 0;
        result.Data.MetricType = metric;
        result.Data.Period = period;
        return result;
    }

    public async Task<ConnectorResponse<HighscoreLeaderboard>> Highscores(int id, MetricType metric) {
        var request = GetNewRestRequest("{id}/hiscores");

        request.AddParameter("id", id, ParameterType.UrlSegment);
        request.AddParameter("metric", metric.ToEnumMemberOrDefault());

        var requestResult = await ExecuteCollectionRequest<LeaderboardMember>(request);
        var result = GetResponse<HighscoreLeaderboard>(requestResult);

        result.Data.Page = 0;
        result.Data.PageSize = 20;
        result.Data.MetricType = metric;

        foreach (var member in result.Data.Members) {
            member.MetricType = metric;
        }

        return result;
    }

  
    public async Task<ConnectorCollectionResponse<Achievement>> RecentAchievements(int id) {
        var request = GetNewRestRequest("{id}/achievements");

        request.AddParameter("id", id, ParameterType.UrlSegment);

        var result = await ExecuteCollectionRequest<WOMAchievement>(request);
        return GetResponse<WOMAchievement, Achievement>(result);
    }

  public async Task<ConnectorResponse<Group>> AddMembers(int id, string verificationCode, IEnumerable<MemberRequest> members) {
        var restRequest = GetNewRestRequest("{id}/add-members");
        restRequest.AddParameter("id", id, ParameterType.UrlSegment);
        restRequest.Method = Method.Post;
        restRequest.AddJsonBody(new {
            verificationCode, members
        });

        var restResult = await ExecuteRequest<GroupEditResponse>(restRequest);
        return GetResponse<Group>(restResult);
    }


    public async Task<ConnectorResponse<MessageResponse>> Update(int id, string verificationCode) {
        var restRequest = GetNewRestRequest("{id}/update-all");
        restRequest.AddParameter("id", id, ParameterType.UrlSegment);
        restRequest.Method = Method.Post;

        restRequest.AddJsonBody(new {
            verificationCode
        });


        var restResult = await ExecuteRequest<WOMMessageResponse>(restRequest);
        return GetResponse<MessageResponse>(restResult);
    }

}
