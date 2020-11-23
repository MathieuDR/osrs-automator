using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.Output.Exceptions;
using WiseOldManConnector.Models.WiseOldMan.Enums;
using WiseOldManConnectorTests.Configuration;
using WiseOldManConnectorTests.Fixtures;
using Xunit;
using Record = WiseOldManConnector.Models.Output.Record;

namespace WiseOldManConnectorTests.Connectors {
    public class PlayerConnectorTests : ConnectorTests {
        public PlayerConnectorTests(ApiFixture fixture) : base(fixture) {
            _playerApi = fixture.ServiceProvider.GetService<IWiseOldManPlayerApi>();
        }

        private readonly IWiseOldManPlayerApi _playerApi;

        [Fact]
        public async Task AchievementByAccomplishedUserIdResultsInCollectionWithMultiple() {
            var username = TestConfiguration.ValidAccomplishedPlayerId;

            var response = await _playerApi.Achievements(username);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
            Assert.True(response.Data.Count() > 1);
        }

        [Fact]
        public async Task AchievementByAccomplishedUsernameResultsInCollectionWithMultiple() {
            var username = TestConfiguration.ValidAccomplishedPlayer;

            var response = await _playerApi.Achievements(username);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
            Assert.True(response.Data.Count() > 1);
        }

        [Fact]
        public async Task AchievementByValidUserIdResultsInCollection() {
            var username = TestConfiguration.ValidPlayerId;

            var response = await _playerApi.Achievements(username);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async Task AchievementByValidUsernameResultsInCollection() {
            var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            var response = await _playerApi.Achievements(username);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async Task AchievementByValidUserResultsInAchievementsWithMissings() {
            var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            var response = await _playerApi.Achievements(username, true);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
            Assert.True(Enum.GetValues(typeof(MetricType)).Length <= response.Data.Count());
        }

        [Fact]
        public async Task AchievementByValidUserIdResultsInAchievementsWithMissings() {
            var id = TestConfiguration.ValidPlayerId;

            var response = await _playerApi.Achievements(id, true);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
            Assert.True(Enum.GetValues(typeof(MetricType)).Length <= response.Data.Count());
        }

        [Fact]
        public async Task AchievementsAreCorrectlyMapped() {
            var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            var response = await _playerApi.Achievements(username);
            var achievement = response.Data.FirstOrDefault();

            Assert.NotNull(achievement);
            Assert.False(achievement.IsMissing);
            Assert.NotEmpty(achievement.Title);
            Assert.True(achievement.Threshold > 1);
            Assert.True(achievement.PlayerId > 0);
        }


        [Fact]
        public async Task AchievementsWithMissingsResultsInCollectionWithAllMetricsAndCombat() {
            var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            var response = await _playerApi.Achievements(username, true);
            var enumValues = EnumHelper.GetMetricTypes(MetricTypeCategory.All).ToList();
            var achievementMetrics = response.Data.Select(x => x.Metric).Distinct().ToList();
            enumValues.Add(MetricType.Combat);

            // Distinct
            Assert.True(achievementMetrics.Count == enumValues.Count);
            foreach (var type in enumValues) {
                Assert.Contains(type, achievementMetrics);
            }
        }

        [Fact]
        public async Task CompetitionByIdResultsInCollection() {
            var id = TestConfiguration.ValidPlayerId;

            ConnectorCollectionResponse<Competition> response = await _playerApi.Competitions(id);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async Task CompetitionMappingIsCorrect() {
            var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            ConnectorCollectionResponse<Competition> response = await _playerApi.Competitions(username);
            Competition competition = response.Data.FirstOrDefault();

            Assert.Contains(response.Data, c => c.GroupId.HasValue);
            Assert.NotNull(competition);
            Assert.NotEmpty(competition.Title);
            Assert.True(competition.Id > 0);
            Assert.False(competition.StartDate == DateTimeOffset.MinValue);
            Assert.False(competition.EndDate == DateTimeOffset.MinValue);
            Assert.False(competition.CreateDate == DateTimeOffset.MinValue);
            
            Assert.True(competition.StartDate < competition.EndDate);
            Assert.NotEmpty(competition.Duration);
            Assert.True(competition.ParticipantCount > 1);
        }

        [Fact]
        public async Task CompetitionsByUsernameResultsIntoCollection() {
            var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            ConnectorCollectionResponse<Competition> response = await _playerApi.Competitions(username);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async Task CompetitionWithInvalidUserResultsIntoExceptionWithMessage() {
            var username = TestConfiguration.InvalidUser;

            Task Act() => _playerApi.Competitions(username);
            var exception = await Assert.ThrowsAnyAsync<BadRequestException>(Act);

            Assert.NotNull(exception);
            Assert.NotEmpty(exception.WiseOldManMessage);
        }

        // Cannot Test
        [Fact]
        public async Task DisplayNameAssertionIsCorrectlyCapatalized() {
            var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization.ToLowerInvariant();

            var response = await _playerApi.AssertDisplayName(username);
            Assert.Equal(TestConfiguration.ValidPlayerUsernameWithValidCapatilization, response.Data);
        }

        //[Fact]
        //public async Task DisplayNameAssertionByCorrectDisplayNameThrowsException() {
        //    var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization.ToLowerInvariant();

        //    Task Act() => _playerApi.AssertDisplayName(username);
        //    var error = await Assert.ThrowsAsync<BadRequestException>(Act);
        //    Assert.Contains("No change required", error.Message); 
        //}



        [Fact]
        public void DisplayNameAssertionWithoutUsernameThrowsException() {
            var username = "";

            Task Act() => _playerApi.AssertDisplayName(username);
            Assert.ThrowsAsync<BadRequestException>(Act);
        }

        [Fact]
        public void ImportingPlayerWithoutUsernameResultsIntoException() {
            string username = "";
            Task Act() => _playerApi.Import(username);
            Assert.ThrowsAsync<BadRequestException>(Act);
        }

        [Fact]
        public async Task ImportValidPlayerResultsWithNoExceptionAndMessageResult() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;
            try {
                ConnectorResponse<MessageResponse> response = await _playerApi.Import(username);
                Assert.NotNull(response);
                Assert.NotNull(response.Data);
                Assert.NotEmpty(response.Data.Message);
            } catch (BadRequestException e) {
                if (!e.WiseOldManMessage.Contains("Failed to load history from CML.") && !e.WiseOldManMessage.Contains("Imported too soon,")) {
                    // If it contains above string, CML is unresponsive and cannot test.
                    throw;
                }
            }
        }

        [Fact]
        public async Task SearchPlayerWithInvalidUsernameResultsEmptyCollection() {
            string username = "sghsdfgwe";

            ConnectorCollectionResponse<Player> response = await _playerApi.Search(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Empty(response.Data);
        }

        [Fact]
        public async Task SearchPlayerWithNoUsernameResultsIntoException() {
            string username = "";
            Task Act() => _playerApi.Search(username);
            await Assert.ThrowsAsync<BadRequestException>(Act);
        }

        [Fact]
        public async Task SearchPlayerWithSpecificUsernameResultsIntoCollectionWithOnePlayer() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            ConnectorCollectionResponse<Player> response = await _playerApi.Search(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.NotEmpty(response.Data);
            Assert.Contains(response.Data, x => String.Equals(x.Username, username, StringComparison.InvariantCultureIgnoreCase));
            Assert.True(response.Data.Count() == 1);
        }

        [Fact]
        public async Task SearchPlayerResultsInValidPlayer() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            ConnectorCollectionResponse<Player> response = await _playerApi.Search(username);

           
            var player = response.Data.FirstOrDefault();
            
            Assert.NotEmpty(player.DisplayName);
            Assert.NotEmpty(player.Username);
            Assert.True(player.Id > 0);
            Assert.Null(player.LatestSnapshot);
            Assert.Null(player.Role);
            Assert.Equal(0, player.OverallExperience);
            Assert.True(player.UpdatedAt < DateTimeOffset.Now);
            Assert.True(player.RegisteredAt < DateTimeOffset.Now);
        }

        [Fact]
        public async Task SearchPlayerWithUnspecificUserNameResultsInCollectionWithMultiplePlayers() {
            string username = "iron";

            ConnectorCollectionResponse<Player> response = await _playerApi.Search(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.NotEmpty(response.Data);
            Assert.True(response.Data.Count() > 1);
        }

        [Fact]
        public async Task SearchPlayerWithUsernameResultsIntoCollectionWithPlayer() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            ConnectorCollectionResponse<Player> response = await _playerApi.Search(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.NotEmpty(response.Data);
            Assert.Contains(response.Data, x => String.Equals(x.Username, username, StringComparison.InvariantCultureIgnoreCase));
        }

        [Fact]
        public async Task SnapshotByIdAndDayPeriodResultsInMultipleSnapshots() {
            int id = TestConfiguration.ValidPlayerId;

            ConnectorCollectionResponse<Snapshot> response = await _playerApi.Snapshots(id, Period.Day);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async Task SnapshotByIdAndMonthPeriodResultsInMultipleSnapshots() {
            int id = TestConfiguration.ValidPlayerId;

            var response = await _playerApi.Snapshots(id, Period.Month);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async Task SnapshotByIdAndWeekPeriodResultsInMultipleSnapshots() {
            int id = TestConfiguration.ValidPlayerId;

            var response = await _playerApi.Snapshots(id, Period.Week);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async Task SnapshotByIdAndYearPeriodResultsInMultipleSnapshots() {
            int id = TestConfiguration.ValidPlayerId;

            var response = await _playerApi.Snapshots(id, Period.Year);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        //[Fact]
        //public async Task SnapshotByIdResultsInMultipleSnapshots() {
        //    int id = TestConfiguration.ValidPlayerId;

        //    ConnectorResponse<Snapshots> response = await _playerApi.Snapshots(id);

        //    Assert.NotNull(response);
        //    Assert.NotNull(response.Data);
        //    Assert.NotEmpty(response.Data.Day);
        //    Assert.NotEmpty(response.Data.Week);
        //    Assert.NotEmpty(response.Data.Month);
        //    Assert.NotEmpty(response.Data.Year);
        //    Assert.NotEmpty(response.Data.Combined);
        //    Assert.True(response.Data.Combined.Count == (response.Data.Week.Count + response.Data.Day.Count +
        //                                                 response.Data.Month.Count + response.Data.Year.Count));
        //}

        [Fact]
        public async Task SnapshotByUsernameAndDayPeriodResultsInMultipleSnapshots() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            var response = await _playerApi.Snapshots(username, Period.Day);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async Task SnapshotByUsernameAndMonthPeriodResultsInMultipleSnapshots() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            var response = await _playerApi.Snapshots(username, Period.Month);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async Task SnapshotByUsernameAndWeekPeriodResultsInMultipleSnapshots() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            var response = await _playerApi.Snapshots(username, Period.Week);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async Task SnapshotByUsernameAndYearPeriodResultsInMultipleSnapshots() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            var response = await _playerApi.Snapshots(username, Period.Year);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        //[Fact]
        //public async Task SnapshotByUsernameResultsInMultipleSnapshots() {
        //    string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

        //    ConnectorResponse<Snapshots> response = await _playerApi.Snapshots(username);

        //    Assert.NotNull(response);
        //    Assert.NotNull(response.Data);
        //    Assert.NotEmpty(response.Data.Day);
        //    Assert.NotEmpty(response.Data.Week);
        //    Assert.NotEmpty(response.Data.Month);
        //    Assert.NotEmpty(response.Data.Year);
        //    Assert.NotEmpty(response.Data.Combined);
        //    Assert.True(response.Data.Combined.Count == (response.Data.Week.Count + response.Data.Day.Count +
        //                                                 response.Data.Month.Count + response.Data.Year.Count));
        //}

        //[Fact]
        //public async Task SnapshotByUsernameHoldsAllMetrics() {
        //    string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

        //    var response = await _playerApi.Snapshots(username);

        //    var types = EnumHelper.GetMetricTypes(MetricTypeCategory.All);
        //    var typesInResponse = response.Data.Combined.SelectMany(x => x.AllMetrics.Select(x=>x.Key)).Distinct().ToList();
        //    foreach (var type in types) {
        //        Assert.Contains(type, typesInResponse);
        //    }
        //}

        [Fact]
        public async Task TrackingMultipleTimesInShortPeriodResultsInException() {
            string username = TestConfiguration.SecondaryValidPlayerUserName;
            ConnectorResponse<Player> response = await _playerApi.Track(username);
            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(username, response.Data.Username, StringComparer.InvariantCultureIgnoreCase);

            Thread.Sleep(10000);
            Task Act() => _playerApi.Track(username);
            await Assert.ThrowsAsync<BadRequestException>(Act);
        }

        [Fact]
        public async Task TrackingPlayerByUsernameResultsInPlayer() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;
            ConnectorResponse<Player> response = await _playerApi.Track(username);
            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(username, response.Data.Username, StringComparer.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task TrackingPlayerWithoutUsernameResultsInException() {
            string username = "";
            Task Act() => _playerApi.Track(username);
            await Assert.ThrowsAsync<BadRequestException>(Act);
        }

        [Fact]
        public Task TypeAssertionForHardcoreIronmanIsCorrect() {
            var username = TestConfiguration.ValidHardcoreIronMan;

            var response = _playerApi.AssertPlayerType(username);
            Assert.Equal(PlayerType.HardcoreIronMan, response.Result.Data);

            return Task.CompletedTask;
        }

        [Fact]
        public Task TypeAssertionForHardcoreIronmanIsWrong() {
            var username = TestConfiguration.ValidRegularPlayer;

            var response = _playerApi.AssertPlayerType(username);
            Assert.NotEqual(PlayerType.HardcoreIronMan, response.Result.Data);

            return Task.CompletedTask;
        }

        [Fact]
        public Task TypeAssertionForIronmanIsCorrect() {
            var username = TestConfiguration.ValidIronMan;

            var response = _playerApi.AssertPlayerType(username);
            Assert.Equal(PlayerType.IronMan, response.Result.Data);
       
            return Task.CompletedTask;
        }

        [Fact]
        public Task TypeAssertionForIronmanIsWrong() {
            var username = TestConfiguration.ValidRegularPlayer;

            var response = _playerApi.AssertPlayerType(username);
            Assert.NotEqual(PlayerType.IronMan, response.Result.Data);
        
            return Task.CompletedTask;
        }

        [Fact]
        public Task TypeAssertionForRegularPlayerIsCorrect() {
            var username = TestConfiguration.ValidRegularPlayer;

            var response = _playerApi.AssertPlayerType(username);
            Assert.Equal(PlayerType.Regular, response.Result.Data);
        
            return Task.CompletedTask;
        }

        [Fact]
        public Task TypeAssertionForRegularPlayerIsWrong() {
            var username = TestConfiguration.ValidHardcoreIronMan;

            var response = _playerApi.AssertPlayerType(username);
            Assert.NotEqual(PlayerType.Regular, response.Result.Data);
        
            return Task.CompletedTask;
        }

        [Fact]
        public Task TypeAssertionForUltimateIronManIsCorrect() {
            var username = TestConfiguration.ValidUltimateIronMan;

            var response = _playerApi.AssertPlayerType(username);
            Assert.Equal(PlayerType.UltimateIronMan, response.Result.Data);
        
            return Task.CompletedTask;
        }

        [Fact]
        public Task TypeAssertionForUltimateIronManIsWrong() {
            var username = TestConfiguration.ValidRegularPlayer;

            var response = _playerApi.AssertPlayerType(username);
            Assert.NotEqual(PlayerType.UltimateIronMan, response.Result.Data);
        
            return Task.CompletedTask;
        }

        [Fact]
        public async Task ViewPlayerByIdResultsIntoPlayerResult() {
            int id = TestConfiguration.ValidPlayerId;

            ConnectorResponse<Player> response = await _playerApi.View(id);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(id, response.Data.Id);
        }

        [Fact]
        public async Task ViewPlayerByIdResultsInValidPlayer() {
            int id = TestConfiguration.ValidPlayerId;

            ConnectorResponse<Player> response = await _playerApi.View(id);

            var player = response.Data;

            Assert.NotEmpty(player.DisplayName);
            Assert.NotEmpty(player.Username);
            Assert.True(player.Id > 0);
            Assert.True(player.CombatLevel > 3);
            Assert.NotNull(player.LatestSnapshot);
            Assert.True(player.UpdatedAt < DateTimeOffset.Now);
            Assert.True(player.RegisteredAt < DateTimeOffset.Now);
        }


        [Fact]
        public void ViewPlayerByInvalidIdResultsInBadRequestException() {
            int id = -1;

            Task Act() => _playerApi.View(id);

            Assert.ThrowsAsync<BadRequestException>(Act);
        }

        [Fact]
        public async Task ViewPlayerByUsernameResultsIntoPlayerWithSameUsername() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            ConnectorResponse<Player> response = await _playerApi.View(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(username, response.Data.Username, StringComparer.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task ViewPlayerSnapshotHasAllMetrics() {
            int id = TestConfiguration.ValidPlayerId;

            ConnectorResponse<Player> response = await _playerApi.View(id);


            var types = EnumHelper.GetMetricTypes(MetricTypeCategory.All);
            foreach (var type in types) {
                Assert.Contains(type, (IDictionary<MetricType, Metric>) response.Data.LatestSnapshot.AllMetrics);
            }
        }

        [Fact]
        public async Task GainedByUserIdResultsInCollection() {
            int id = TestConfiguration.ValidPlayerId;

            var response = await _playerApi.Gained(id);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
            Assert.True(response.Data.Count() == 4);
        }

        [Fact]
        public async Task GainedByUsernameResultsInCollection() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            var response = await _playerApi.Gained(username);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
            Assert.True(response.Data.Count() == 4);
        }

        [Theory]
        [InlineData(Period.Day)]
        [InlineData(Period.Week)]
        [InlineData(Period.Month)]
        [InlineData(Period.Year)]
        public async Task GainedByUserIdAndPeriodResultsInCollection(Period period) {
            int id = TestConfiguration.ValidPlayerId;

            var response = await _playerApi.Gained(id, period);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(period, response.Data.Period);
        }

        [Theory]
        [InlineData(Period.Day)]
        [InlineData(Period.Week)]
        [InlineData(Period.Month)]
        [InlineData(Period.Year)]
        public async Task GainedByUsernameAndPeriodResultsInCollection(Period period) {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;
            var response = await _playerApi.Gained(username, period);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(period, response.Data.Period);
        }

        [Theory]
        [InlineData(new object[] {"ErkendRserke",null, null})]
        [InlineData(new object[] {"ErkendRserke", MetricType.Fishing, null})]
        [InlineData(new object[] {"ErkendRserke", null, Period.Week})]
        [InlineData(new object[] {"ErkendRserke", MetricType.Thieving, Period.Month})]
        public async Task RecordsByUsernameAndParametersResultInCollection(string username, MetricType? metric, Period? period) {
            ConnectorCollectionResponse<Record> response;

            if (metric.HasValue && period.HasValue) {
                response = await _playerApi.Records(username, metric.Value, period.Value);
            }else if (metric.HasValue) {
                response = await _playerApi.Records(username, metric.Value);
            }else if (period.HasValue) {
                response = await _playerApi.Records(username, period.Value);
            } else {
                response = await _playerApi.Records(username);
            }

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);

            //No player Data!
            Assert.Empty(response.Data.Where(x=>x.Player == null));
            Assert.Equal(response.Data.Count(), response.Data.Count(x => x.Player.Username == username));

            if (metric.HasValue) {
                var nonMetric = response.Data.Where(x => x.MetricType != metric.Value).ToList();
                Assert.Empty(nonMetric);
            }

            if (period.HasValue) {
                var nonPeriod = response.Data.Where(x => x.Period != period.Value).ToList();
                Assert.Empty(nonPeriod);
            }
        }

        [Theory]
        [InlineData(new object[] {4029,null, null})]
        [InlineData(new object[] {4029, MetricType.Fishing, null})]
        [InlineData(new object[] {4029, null, Period.Week})]
        [InlineData(new object[] {4029, MetricType.Fishing, Period.Week})]
        public async Task RecordsByUserIdAndParametersResultInCollection(int id, MetricType? metric, Period? period) {
            ConnectorCollectionResponse<Record> response;

            if (metric.HasValue && period.HasValue) {
                response = await _playerApi.Records(id, metric.Value, period.Value);
            }else if (metric.HasValue) {
                response = await _playerApi.Records(id, metric.Value);
            }else if (period.HasValue) {
                response = await _playerApi.Records(id, period.Value);
            } else {
                response = await _playerApi.Records(id);
            }

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);

            if (metric.HasValue) {
                var nonMetric = response.Data.Where(x => x.MetricType != metric.Value).ToList();
                Assert.Empty(nonMetric);
            }

            if (period.HasValue) {
                var nonPeriod = response.Data.Where(x => x.Period != period.Value).ToList();
                Assert.Empty(nonPeriod);
            }
        }

    }
}