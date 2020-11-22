using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.Requests;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Api {
    internal class GroupConnector : BaseConnecter, IWiseOldManGroupApi {
        public GroupConnector(IServiceProvider provider) : base(provider) {
            Area = "groups";
        }

        protected override string Area { get; }

        public async Task<ConnectorCollectionResponse<Group>> Search() {
            var request = GetNewRestRequest("");

            var result = await ExecuteCollectionRequest<WOMGroup>(request);
            return GetResponse<WOMGroup, Group>(result);
        }

        public async Task<ConnectorCollectionResponse<Group>> Search(string name) {
            var request = GetNewRestRequest("");

            request.AddParameter("name", name);

            var result = await ExecuteCollectionRequest<WOMGroup>(request);
            return GetResponse<WOMGroup, Group>(result);
        }

        public async Task<ConnectorCollectionResponse<Group>> Search(string name, int limit, int offset) {
            var request = GetNewRestRequest("");

            request.AddParameter("name", name);
            AddPaging(request, limit, offset);

            var result = await ExecuteCollectionRequest<WOMGroup>(request);
            return GetResponse<WOMGroup, Group>(result);
        }

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

        public async Task<ConnectorResponse<DeltaMember>> GetMonthTopMember(int id) {
            var request = GetNewRestRequest("{id}/monthly-top");

            request.AddParameter("id", id, ParameterType.UrlSegment);

            var result = await ExecuteRequest<WOMGroupDeltaMember>(request);
            return GetResponse<DeltaMember>(result);
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
            request.AddParameter("metric", metric.GetEnumValueNameOrDefault());
            request.AddParameter("period", period.GetEnumValueNameOrDefault());

            var requestResult = await ExecuteCollectionRequest<WOMGroupDeltaMember>(request);
            var result = GetResponse<DeltaLeaderboard>(requestResult);
            result.Data.PageSize = 20;
            result.Data.Page = 0;
            result.Data.MetricType = metric;
            result.Data.Period = period;
            return result;
        }

        public async Task<ConnectorResponse<DeltaLeaderboard>> GainedLeaderboards(int id, MetricType metric, Period period,
            int limit, int offset) {
            var request = GetNewRestRequest("{id}/gained");

            request.AddParameter("id", id, ParameterType.UrlSegment);
            request.AddParameter("metric", metric.GetEnumValueNameOrDefault());
            request.AddParameter("period", period.GetEnumValueNameOrDefault());
            AddPaging(request, limit, offset);

            var requestResult = await ExecuteCollectionRequest<WOMGroupDeltaMember>(request);
            var result = GetResponse<DeltaLeaderboard>(requestResult);
            result.Data.PageSize = limit;
            result.Data.Page = offset;
            result.Data.MetricType = metric;
            result.Data.Period = period;
            return result;
        }

        public async Task<ConnectorResponse<HighscoreLeaderboard>> Highscores(int id, MetricType metric) {
            var request = GetNewRestRequest("{id}/hiscores");

            request.AddParameter("id", id, ParameterType.UrlSegment);
            request.AddParameter("metric", metric.GetEnumValueNameOrDefault());

            var requestResult = await ExecuteCollectionRequest<LeaderboardMember>(request);
            var result = GetResponse<HighscoreLeaderboard>(requestResult);

            result.Data.Page = 0;
            result.Data.PageSize = 20;

            foreach (HighscoreMember member in result.Data.Members) {
                member.MetricType = metric;
            }

            return result;
        }

        public async Task<ConnectorResponse<HighscoreLeaderboard>> Highscores(int id, MetricType metric, int limit, int offset) {
            var request = GetNewRestRequest("{id}/hiscores");

            request.AddParameter("id", id, ParameterType.UrlSegment);
            request.AddParameter("metric", metric.GetEnumValueNameOrDefault());
            AddPaging(request, limit, offset);

            var requestResult = await ExecuteCollectionRequest<LeaderboardMember>(request);
            var result = GetResponse<HighscoreLeaderboard>(requestResult);

            result.Data.Page = offset;
            result.Data.PageSize = limit;

            foreach (HighscoreMember member in result.Data.Members) {
                member.MetricType = metric;
            }

            return result;
        }

        public async Task<ConnectorResponse<RecordLeaderboard>> RecordLeaderboards(int id, MetricType metric, Period period) {
            var request = GetNewRestRequest("{id}/records");

            request.AddParameter("id", id, ParameterType.UrlSegment);
            request.AddParameter("metric", metric.GetEnumValueNameOrDefault());
            request.AddParameter("period", period.GetEnumValueNameOrDefault());

            var requestResult = await ExecuteCollectionRequest<LeaderboardMember>(request);
            var result = GetResponse<RecordLeaderboard>(requestResult);

            result.Data.PageSize = 20;
            result.Data.Page = 0;
            result.Data.Period = period;
            result.Data.MetricType = metric;

            return result;
        }

        public async Task<ConnectorResponse<RecordLeaderboard>> RecordLeaderboards(int id, MetricType metric, Period period,
            int limit, int offset) {
            var request = GetNewRestRequest("{id}/records");

            request.AddParameter("id", id, ParameterType.UrlSegment);
            request.AddParameter("metric", metric.GetEnumValueNameOrDefault());
            request.AddParameter("period", period.GetEnumValueNameOrDefault());
            AddPaging(request, limit, offset);

            var requestResult = await ExecuteCollectionRequest<LeaderboardMember>(request);
            var result = GetResponse<RecordLeaderboard>(requestResult);

            result.Data.PageSize = limit;
            result.Data.Page = offset;
            result.Data.Period = period;
            result.Data.MetricType = metric;

            return result;
        }

        public async Task<ConnectorCollectionResponse<Achievement>> RecentAchievements(int id) {
            var request = GetNewRestRequest("{id}/achievements");

            request.AddParameter("id", id, ParameterType.UrlSegment);

            var result = await ExecuteCollectionRequest<WOMAchievement>(request);
            return GetResponse<WOMAchievement, Achievement>(result);
        }

        public async Task<ConnectorCollectionResponse<Achievement>> RecentAchievements(int id, int limit, int offset) {
            var request = GetNewRestRequest("{id}/achievements");

            request.AddParameter("id", id, ParameterType.UrlSegment);
            AddPaging(request, limit, offset);

            var result = await ExecuteCollectionRequest<WOMAchievement>(request);
            return GetResponse<WOMAchievement, Achievement>(result);
        }

        public async Task<ConnectorResponse<Statistics>> Statistics(int id) {
            var request = GetNewRestRequest("{id}/statistics");

            request.AddParameter("id", id, ParameterType.UrlSegment);

            var result = await ExecuteRequest<StatisticsResponse>(request);
            return GetResponse<Statistics>(result);
        }

        public async Task<ConnectorResponse<VerificationGroup>> Create(CreateGroupRequest request) {
            var restRequest = GetNewRestRequest();
            restRequest.Method = Method.POST;
            restRequest.AddJsonBody(request);

            var restResult = await ExecuteRequest<GroupCreateResponse>(restRequest);
            return GetResponse<VerificationGroup>(restResult);
        }

        public async Task<ConnectorResponse<Group>> Edit(int id, EditGroupRequest request) {
            var restRequest = GetNewRestRequest("{id}");
            restRequest.AddParameter("id", id, ParameterType.UrlSegment);
            restRequest.Method = Method.PUT;
            restRequest.AddJsonBody(request);

            var restResult = await ExecuteRequest<GroupEditResponse>(restRequest);
            return GetResponse<Group>(restResult);
        }

        public async Task<ConnectorResponse<MessageResponse>> Delete(int id, string verificationCode) {
            var restRequest = GetNewRestRequest("{id}");
            restRequest.AddParameter("id", id, ParameterType.UrlSegment);
            restRequest.Method = Method.DELETE;
            restRequest.AddJsonBody(new {verificationCode});

            var restResult = await ExecuteRequest<WOMMessageResponse>(restRequest);
            return GetResponse<MessageResponse>(restResult);
        }

        public async Task<ConnectorResponse<Group>> AddMembers(int id, string verificationCode, IEnumerable<MemberRequest> members) {
            var restRequest = GetNewRestRequest("{id}/add-members");
            restRequest.AddParameter("id", id, ParameterType.UrlSegment);
            restRequest.Method = Method.POST;
            restRequest.AddJsonBody(new {
                verificationCode, members
            });

            var restResult = await ExecuteRequest<GroupEditResponse>(restRequest);
            return GetResponse<Group>(restResult);
        }

        public Task<ConnectorResponse<Group>> RemoveMembers(int id, string verificationCode, IEnumerable<string> members) {
            throw new NotImplementedException();
        }

        public Task<ConnectorResponse<Player>> ChangeMemberRole(int id, string verificationCode, string username,
            GroupRole role) {
            throw new NotImplementedException();
        }

        public Task<ConnectorResponse<MessageResponse>> Update(int id) {
            throw new NotImplementedException();
        }

        private void AddPaging(RestRequest request, int limit, int offset) {
            request.AddParameter("limit", limit);
            request.AddParameter("offset", offset);
        }
    }
}