using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using DiscordBotFanatic.Models.Data;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses;
using DiscordBotFanatic.Repository;
using DiscordBotFanatic.Services.interfaces;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace DiscordBotFanatic.Services
{
    public class WiseOldManConsumer {
        private readonly ILogService _logger;
        private readonly IDiscordBotRepository _repository;
        private const string BaseUrl = "https://wiseoldman.net/api";
        private const string PlayersBase = "players";
        private const string RecordsBase = "records";
        private const string DeltaBase = "deltas";
        private readonly RestClient _client;

        public WiseOldManConsumer(ILogService logger, IDiscordBotRepository repository) {
            _logger = logger;
            _repository = repository;
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
            IEnumerable<SearchResponse> result = await _client.GetAsync<IEnumerable<SearchResponse>>(request);

            return result;
        }

        public async Task<PlayerResponse> GetPlayerAsync(string username) {
            var request = new RestRequest($"{PlayersBase}", DataFormat.Json);
            request.AddParameter("username", username);

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

        public async Task<DeltaResponse> DeltaPlayerAsync(int id, Period period = Period.Week) {
            var request = new RestRequest($"{DeltaBase}", DataFormat.Json);
            request.AddParameter("playerId", id);
            request.AddParameter("period", period.ToString().ToLower());
            
            LogRequest(request, MethodBase.GetCurrentMethod()?.Name);
            var result = await _client.GetAsync<DeltaResponse>(request);

            ValidateResponse(result);
            return result;
        }

        public async Task<PlayerResponse> GetPlayerAsync(int id) {
            var request = new RestRequest($"{PlayersBase}", DataFormat.Json);
            request.AddParameter("id", id);

            LogRequest(request, MethodBase.GetCurrentMethod()?.Name);
            var result = await _client.GetAsync<PlayerResponse>(request);

            ValidateResponse(result);
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

            var result = (await _client.ExecuteAsync<RecordResponse>(request)).Data;
            ValidateResponse(result);
            return result;
        }

        public string GetUserNameFromUser(IUser user) {
            Player player = _repository.GetPlayerByDiscordId(user.Id);
            return player?.DefaultPlayerUsername;
        }

        public async void SetDefaultPlayer(ulong userId, string username) {
            Player fromDb = _repository.GetPlayerByDiscordId(userId);
            if (fromDb != null && fromDb.DefaultPlayerUsername.ToLowerInvariant() == username.ToLowerInvariant()) {
                // Already set
                return;
            }

            // Search all players
            var players = (await SearchPlayerAsync(username)).ToList();

            PlayerResponse player;
            Player dbPlayer;
            switch (players.Count) {
                case 0:
                    // Track player if we cant find him
                    player = await TrackPlayerAsync(username);
                    dbPlayer = new Player() {
                        DiscordId = userId, DefaultPlayerUsername = player.Username,
                        WiseOldManDefaultPlayerId = player.Id
                    };
                    break;
                case 1: {
                    var result = players.First();
                    dbPlayer = new Player() {
                        DiscordId = userId, DefaultPlayerUsername = result.Username,
                        WiseOldManDefaultPlayerId = result.Id
                    };
                    break;
                }
                default:
                    throw new ArgumentException($"Please use your full OSRS name. Too many results using {username} ({players.Count}).");
            }


            _repository.InsertOrUpdatePlayer(dbPlayer);
        }

        private void ValidateResponse(BaseResponse response) {
            if (response == null) {
                throw new ArgumentException($"We did not receive a response. Pleas try again later or contact the administration.");
            }

            if (!string.IsNullOrEmpty(response.Message)) {
                throw new ArgumentException(response.Message);
            }
        }
    }
}