using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace DiscordBotFanatic.Services
{
    public class WiseOldManConsumer {
        private readonly LogService _logger;
        private const string BaseUrl = "https://wiseoldman.net/api";
        private const string PlayersBase = "players";
        private const string RecordsBase = "records";
        private const string DeltaBase = "deltas";
        private readonly RestClient _client;

        public WiseOldManConsumer(LogService logger) {
            _logger = logger;
            _client = new RestClient(BaseUrl);
            _client.UseNewtonsoftJson();
        }


        private void LogRequest(RestRequest request, string source = nameof(WiseOldManConsumer)) {
            string message = $"Request url: {request.Resource}";

            if (request.Parameters != null ) {
                foreach (Parameter parameter in request.Parameters) {
                    message += $"{Environment.NewLine} Request Parameter ({parameter.Name}, {parameter.Value})";
                }
            }

            _logger.LogDebug(new LogMessage(LogSeverity.Info, source, message));
        }

        // might be broken if username results into more then one players.
        public async Task<IEnumerable<SearchResponse>> SearchPlayerAsync(string username) {
            var request = new RestRequest($"{PlayersBase}/search", DataFormat.Json);
            request.AddParameter("username", username);

            LogRequest(request, MethodBase.GetCurrentMethod()?.Name);
            var result = await _client.GetAsync<IEnumerable<SearchResponse>>(request);

            return result;
        }

        public async Task<PlayerResponse> GetPlayerAsync(string username) {
            var request = new RestRequest($"{PlayersBase}", DataFormat.Json);
            request.AddParameter("username", username);

            LogRequest(request, MethodBase.GetCurrentMethod()?.Name);
            var result = await _client.GetAsync<PlayerResponse>(request);

            return result;
        }

        public async Task<PlayerResponse> TrackPlayerAsync(string username) {
            var request = new RestRequest($"{PlayersBase}/track", DataFormat.Json);
            request.AddJsonBody(new {username});

            LogRequest(request, MethodBase.GetCurrentMethod()?.Name);
            var result = await _client.PostAsync<PlayerResponse>(request);

            return result;
        }

        public async Task<DeltaResponse> DeltaPlayerAsync(int id, Period period = Period.Week) {
            var request = new RestRequest($"{DeltaBase}", DataFormat.Json);
            request.AddParameter("playerId", id);

            if (period != Period.Day) {
                request.AddParameter("period", period.ToString().ToLower());
            }
            
            LogRequest(request, MethodBase.GetCurrentMethod()?.Name);
            var result = await _client.GetAsync<DeltaResponse>(request);

            return result;
        }

        public async Task<PlayerResponse> GetPlayerAsync(int id) {
            var request = new RestRequest($"{PlayersBase}", DataFormat.Json);
            request.AddParameter("id", id);

            LogRequest(request, MethodBase.GetCurrentMethod()?.Name);
            var result = await _client.GetAsync<PlayerResponse>(request);

            return result;
        }

        public async Task<RecordResponse> GetPlayerRecordAsync(int id, MetricType? metric, Period? period) {
            var request = new RestRequest($"{RecordsBase}", DataFormat.Json);
            
            request.AddParameter("playerId", id);
            request.Method = Method.GET;

            if (metric.HasValue) {
                request.AddParameter("metric", metric.ToString()?.ToLowerInvariant());
            }
            if (period.HasValue) {
                request.AddParameter("period", period.ToString()?.ToLowerInvariant());
            }

            LogRequest(request, MethodBase.GetCurrentMethod()?.Name);
            var t = await _client.ExecuteAsync<RecordResponse>(request);

            return t.Data;
        }
    }
}