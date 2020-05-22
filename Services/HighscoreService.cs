using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using DiscordBotFanatic.Models.Data;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;
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

        public async Task<Dictionary<GroupMember, DeltaMetric>> GetPlayerRecordsForGroupAsync(MetricType metricType, Period period, int groupId) {
            GroupMembersListResponse memberList = await _highscoreApiRepository.GetPlayersFromGroupId(groupId);
            Dictionary<GroupMember, DeltaMetric> result = new Dictionary<GroupMember, DeltaMetric>();

            //ConcurrentBag<Tuple<GroupMember, DeltaMetric>> concurrentBag = new ConcurrentBag<Tuple<GroupMember, DeltaMetric>>();
            //ParallelLoopResult loop = Parallel.ForEach(memberList.Members, new ParallelOptions() { MaxDegreeOfParallelism = 8 }, async groupMember =>
            //{
            //    DeltaResponse response = await DeltaPlayerAsync(groupMember.Id, period, metricType);
            //    DeltaMetric delta = response.Metrics.MetricFromEnum(metricType);
            //    if (delta != null)
            //    {
            //        concurrentBag.Add(new Tuple<GroupMember, DeltaMetric>(groupMember, delta));
            //    }
            //});

            Task[] tasks = new Task[memberList.Members.Count];
            for (var i = 0; i < memberList.Members.Count; i++) {
                var member = memberList.Members[i];
                //var t = Task.Factory.StartNew(async () => {
                //    var response = await DeltaPlayerAsync(member.Id, period, metricType);
                //    DeltaMetric delta = response.Metrics.MetricFromEnum(metricType);
                //    return new Tuple<GroupMember, DeltaMetric>(member, delta);
                //});

                var t = DeltaPlayerAsync(member.Id, period, metricType);
                tasks[i] = t;
            }

            Task.WaitAll(tasks);

            foreach (var task in tasks) {
                if (!(task.AsyncState is Tuple<GroupMember, DeltaMetric> data)) {
                    continue;
                }

                if (data.Item2 != null) {
                    result.Add(data.Item1, data.Item2);
                }
            }

            return result;
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