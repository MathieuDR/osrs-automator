using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.API.Responses.Models;
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
            var result =  GetResponse<DeltaLeaderboard>(requestResult);
            result.Data.PageSize = 20;
            result.Data.Page = 0;
            result.Data.MetricType = metric;
            result.Data.Period = period;
            return result;
        }

        public async Task<ConnectorResponse<DeltaLeaderboard>> GainedLeaderboards(int id, MetricType metric, Period period, int limit, int offset) {
            var request = GetNewRestRequest("{id}/gained");

            request.AddParameter("id", id, ParameterType.UrlSegment);
            request.AddParameter("metric", metric.GetEnumValueNameOrDefault());
            request.AddParameter("period", period.GetEnumValueNameOrDefault());
            AddPaging(request, limit, offset);

            var requestResult = await ExecuteCollectionRequest<WOMGroupDeltaMember>(request);
            var result =  GetResponse<DeltaLeaderboard>(requestResult);
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

        public async Task<ConnectorResponse<RecordLeaderboard>> RecordLeaderboards(int id, MetricType metric, Period period, int limit, int offset) {
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

        public async Task<ConnectorResponse<Group>> Create(CreateGroupRequest request) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorResponse<Group>> Edit(EditGroupRequest request) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorResponse<MessageResponse>> Delete(int id, string verificationCode) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorResponse<Group>> AddMembers(string verificationCode, IEnumerable<string> members) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorResponse<Group>> RemoveMembers(string verificationCode, IEnumerable<string> members) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorResponse<Player>> ChangeMemberRole(string verificationCode, string username, GroupRole role) {
            throw new NotImplementedException();
        }

        public async Task<ConnectorResponse<MessageResponse>> Update(string id) {
            throw new NotImplementedException();
        }

        private void AddPaging(RestRequest request, int limit, int offset) {
            request.AddParameter("limit", limit);
            request.AddParameter("offset", offset);
        }
    }
}