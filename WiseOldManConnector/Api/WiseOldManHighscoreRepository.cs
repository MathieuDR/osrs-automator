using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models.API.Requests;
using WiseOldManConnector.Models.API.Responses;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Api {
    public class WiseOldManHighscoreRepository  {
        private const string BaseUrl = "https://wiseoldman.net/api";
        private const string PlayersBase = "players";
        private const string RecordsBase = "records";
        private const string DeltaBase = "deltas";
        private const string GroupBase = "groups";
        private const string CompetitionBase = "competitions";
        private readonly RestClient _client;
        //private readonly ILogService _logger;
        private readonly IWiseOldManLogger _logger;

        public WiseOldManHighscoreRepository(IServiceProvider provider) {
            _logger = provider.GetService(typeof(IWiseOldManLogger)) as IWiseOldManLogger;
            _client = new RestClient(BaseUrl);
            _client.UseNewtonsoftJson();
        }

        private void LogRequest(RestRequest request, string source = nameof(WiseOldManHighscoreRepository)) {
            _logger?.Log(LogLevel.Information, null, "Request sent to Wise old man API. [{Resource}, {Parameters:j}]", request.Resource, request.Parameters);
        }

        // might be broken if username results into more then one players.
        public async Task<IEnumerable<SearchResponse>> SearchPlayerAsync(string username) {
            var request = new RestRequest($"{PlayersBase}/search", DataFormat.Json);
            request.AddParameter("username", username);

            LogRequest(request, MethodBase.GetCurrentMethod()?.Name);
            IEnumerable<SearchResponse> result = await _client.GetAsync<IEnumerable<SearchResponse>>(request);

            return result;
        }

        public async Task<PlayerResponse> GetPlayerAsync(string username) {
            var request = new RestRequest($"{PlayersBase}/username/{{username}}", DataFormat.Json);
            request.AddParameter("username", username, ParameterType.UrlSegment);

            LogRequest(request, MethodBase.GetCurrentMethod()?.Name);
            var result = await _client.GetAsync<PlayerResponse>(request);

            ValidateResponse(result);
            return result;
        }

        public async Task<PlayerResponse> TrackPlayerAsync(string username) {
            var request = new RestRequest($"{PlayersBase}/track", DataFormat.Json);
            request.AddJsonBody(new {username});

            LogRequest(request, MethodBase.GetCurrentMethod()?.Name);
            var result = await _client.PostAsync<PlayerResponse>(request);

            ValidateResponse(result);
            return result;
        }

        public async Task<DeltaResponse> DeltaPlayerAsync(int id, Period period = Period.Week, MetricType? metric = null) {
            var request = new RestRequest($"{PlayersBase}/{{id}}/gained", DataFormat.Json);
            request.AddParameter("id", id, ParameterType.UrlSegment);
            request.AddParameter("period", period.GetEnumValueNameOrDefault().ToLower());
            if (metric.HasValue) {
                request.AddParameter("metric", metric.GetEnumValueNameOrDefault()?.ToLower());
            }

            LogRequest(request, MethodBase.GetCurrentMethod()?.Name);
            var result = await _client.GetAsync<DeltaResponse>(request);

            ValidateResponse(result);
            return result;
        }

        public async Task<PlayerResponse> GetPlayerAsync(int id) {
            var request = new RestRequest($"{PlayersBase}/{{id}}", DataFormat.Json);
            request.AddParameter("id", id, ParameterType.UrlSegment);

            LogRequest(request, MethodBase.GetCurrentMethod()?.Name);
            var result = await _client.GetAsync<PlayerResponse>(request);

            ValidateResponse(result);
            return result;
        }

        public async Task<RecordResponse> GetPlayerRecordAsync(int id, MetricType? metric, Period? period) {
            var request = new RestRequest($"{PlayersBase}/{{id}}/records", DataFormat.Json);

            request.AddParameter("id", id, ParameterType.UrlSegment);
            request.Method = Method.GET;

            if (metric.HasValue) {
                request.AddParameter("metric", metric.GetEnumValueNameOrDefault()?.ToLowerInvariant());
            }

            if (period.HasValue) {
                request.AddParameter("period", period.GetEnumValueNameOrDefault()?.ToLowerInvariant());
            }

            LogRequest(request, MethodBase.GetCurrentMethod()?.Name);

            var result = (await _client.ExecuteAsync<RecordResponse>(request)).Data;
            ValidateResponse(result);
            return result;
        }

        public async Task<GroupUpdateResponse> UpdateGroupAsync(int groupId) {
            var request = new RestRequest($"{GroupBase}/{{id}}/update-all", DataFormat.Json);
            request.AddParameter("id", groupId, ParameterType.UrlSegment);

            LogRequest(request, MethodBase.GetCurrentMethod()?.Name);
            var result = await _client.PostAsync<GroupUpdateResponse>(request);

            return result;
        }

        public async Task<GroupMembersListResponse> GetPlayersFromGroupId(int groupId) {
            var request = new RestRequest($"{GroupBase}/{{id}}/members");
            request.AddParameter("id", groupId, ParameterType.UrlSegment);

            LogRequest(request, MethodBase.GetCurrentMethod()?.Name);
            var result = await _client.GetAsync<GroupMembersListResponse>(request);
            ValidateResponse(result);

            return result;
        }

        public async Task<LeaderboardResponse> GetGroupLeaderboards(MetricType metric, Period period, int groupId) {
            var request = new RestRequest($"{GroupBase}/{{id}}/gained");
            request.AddParameter("id", groupId, ParameterType.UrlSegment);
            request.AddParameter("period", period.GetEnumValueNameOrDefault().ToLower());
            request.AddParameter("metric", metric.GetEnumValueNameOrDefault().ToLower());
            request.AddParameter("limit", 500);

            LogRequest(request, MethodBase.GetCurrentMethod()?.Name);
            var result = await _client.GetAsync<LeaderboardResponse>(request);
            ValidateResponse(result);

            result.RequestedType = metric;
            result.RequestedPeriod = period;

            return result;
        }

        public async Task<CreateGroupCompetitionResult> CreateGroupCompetition(CompetitionRequest competitionRequest) {
            var request = new RestRequest($"{CompetitionBase}");
            //request.AddJsonBody(JsonConvert.SerializeObject(createCompetitionRequest));
            request.AddJsonBody(competitionRequest);

            var result = await _client.PostAsync<CreateGroupCompetitionResult>(request);
            ValidateResponse(result);
            return result;
        }

        public async Task<CompetitionResponse> ViewCompetitionDetails(int id) {
            try {
                var request = new RestRequest($"{CompetitionBase}/{{id}}");
                //request.AddJsonBody(JsonConvert.SerializeObject(createCompetitionRequest));
                request.AddParameter("id", id, ParameterType.UrlSegment);
                request.AddParameter("limit", 500);

                LogRequest(request);
                IRestResponse response = _client.Get(request);
                var result = JsonConvert.DeserializeObject<CompetitionResponse>(response.Content);
                ValidateResponse(result);
                return result;
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        private void ValidateResponse(BaseResponse response) {
            if (response == null) {
                throw new ArgumentException($"We did not receive a response. Please try again later or contact the administration.");
            }

            if (!string.IsNullOrEmpty(response.Message)) {
                throw new ArgumentException(response.Message);
            }
        }
    }
}