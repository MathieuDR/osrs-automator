using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using DiscordBotFanatic.Models.Data;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Cleaned;
using DiscordBotFanatic.Models.WiseOldMan.Responses;
using DiscordBotFanatic.Repository;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Services {
    public class HighscoreService : IOsrsHighscoreService {
        private readonly IHighscoreApiRepository _highscoreApiRepository;
        private readonly IDiscordBotRepository _repository;

        public HighscoreService(IDiscordBotRepository repository, IHighscoreApiRepository highscoreApiRepository) {
            _repository = repository;
            _highscoreApiRepository = highscoreApiRepository;
        }

        public string GetUserNameFromUser(IUser user) {
            Player player = _repository.GetPlayerByDiscordId(user.Id);
            return player?.DefaultPlayerUsername;
        }

        public async Task<List<LeaderboardMemberInfo>> GetPlayerRecordsForGroupAsync(MetricType metricType, Period period, int groupId) {
            var leaderboard = await _highscoreApiRepository.GetGroupLeaderboards(metricType, period, groupId);
            
            return leaderboard.MemberInfos.OrderByDescending(x => x.HasGained).ToList();
        }

        public async void SetDefaultPlayer(ulong userId, string username) {
            Player fromDb = _repository.GetPlayerByDiscordId(userId);
            if (fromDb != null && fromDb.DefaultPlayerUsername.ToLowerInvariant() == username.ToLowerInvariant()) {
                // Already set
                return;
            }

            // Search all players
            var players = (await _highscoreApiRepository.SearchPlayerAsync(username)).ToList();

            PlayerResponse player;
            Player dbPlayer;
            switch (players.Count) {
                case 0:
                    // Track player if we cant find him
                    player = await _highscoreApiRepository.TrackPlayerAsync(username);
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

        public Task<IEnumerable<SearchResponse>> SearchPlayerAsync(string username) {
            return _highscoreApiRepository.SearchPlayerAsync(username);
        }

        public Task<PlayerResponse> GetPlayerAsync(string username) {
            return _highscoreApiRepository.GetPlayerAsync(username);
        }

        public Task<PlayerResponse> TrackPlayerAsync(string username) {
            return _highscoreApiRepository.TrackPlayerAsync(username);
        }

        public Task<DeltaResponse> DeltaPlayerAsync(int id, Period period = Period.Week, MetricType? metricType = null) {
            return _highscoreApiRepository.DeltaPlayerAsync(id, period, metricType);
        }

        public Task<PlayerResponse> GetPlayerAsync(int id) {
            return _highscoreApiRepository.GetPlayerAsync(id);
        }

        public Task<RecordResponse> GetPlayerRecordAsync(int id, MetricType? metric, Period? period) {
            return _highscoreApiRepository.GetPlayerRecordAsync(id, metric, period);
        }

        public Task<GroupUpdateResponse> UpdateGroupAsync(int id) {
            return _highscoreApiRepository.UpdateGroupAsync(id);
        }

        public int GetGroupIdFromName(string name) {
            throw new NotImplementedException();
        }
    }
}