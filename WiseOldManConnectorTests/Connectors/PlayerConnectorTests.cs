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
        public PlayerConnectorTests(APIFixture fixture) : base(fixture) {
            _playerApi = fixture.ServiceProvider.GetService<IWiseOldManPlayerApi>();
        }

        private readonly IWiseOldManPlayerApi _playerApi;

        [Fact]
        public async void AchievementByAccomplishedUserIdResultsInCollectionWithMultiple() {
            var username = TestConfiguration.ValidAccomplishedPlayerId;

            var response = await _playerApi.Achievements(username);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
            Assert.True(response.Data.Count() > 1);
        }

        [Fact]
        public async void AchievementByAccomplishedUsernameResultsInCollectionWithMultiple() {
            var username = TestConfiguration.ValidAccomplishedPlayer;

            var response = await _playerApi.Achievements(username);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
            Assert.True(response.Data.Count() > 1);
        }

        [Fact]
        public async void AchievementByValidUserIdResultsInCollection() {
            var username = TestConfiguration.ValidPlayerId;

            var response = await _playerApi.Achievements(username);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async void AchievementByValidUsernameResultsInCollection() {
            var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            var response = await _playerApi.Achievements(username);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async void AchievementByValidUserResultsInAchievementsWithMissings() {
            var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            var response = await _playerApi.Achievements(username, true);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
            Assert.True(Enum.GetValues(typeof(MetricType)).Length <= response.Data.Count());
        }

        [Fact]
        public async void AchievementByValidUserIdResultsInAchievementsWithMissings() {
            var id = TestConfiguration.ValidPlayerId;

            var response = await _playerApi.Achievements(id, true);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
            Assert.True(Enum.GetValues(typeof(MetricType)).Length <= response.Data.Count());
        }

        [Fact]
        public async void AchievementsAreCorrectlyMapped() {
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
        public async void AchievementsWithMissingsResultsInCollectionWithAllMetricsAndCombat() {
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
        public async void CompetitionByIdResultsInCollection() {
            var id = TestConfiguration.ValidPlayerId;

            ConnectorCollectionResponse<Competition> response = await _playerApi.Competitions(id);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async void CompetitionMappingIsCorrect() {
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
            Assert.True(competition.Participants > 1);
        }

        [Fact]
        public async void CompetitionsByUsernameResultsIntoCollection() {
            var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            ConnectorCollectionResponse<Competition> response = await _playerApi.Competitions(username);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async void CompetitionWithInvalidUserResultsIntoExceptionWithMessage() {
            var username = TestConfiguration.InvalidUser;

            Task Act() => _playerApi.Competitions(username);
            var exception = await Assert.ThrowsAnyAsync<BadRequestException>(Act);

            Assert.NotNull(exception);
            Assert.NotEmpty(exception.WiseOldManMessage);
        }

        // Cannot Test
        //[Fact]
        //public async void DisplayNameAssertionIsCorrectlyCapatalized() {
        //    var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization.ToLowerInvariant();

        //    var response = await _playerApi.AssertDisplayName(username);
        //    Assert.Equal(TestConfiguration.ValidPlayerUsernameWithValidCapatilization, response.Data);
        //}

        [Fact]
        public async void DisplayNameAssertionByCorrectDisplayNameThrowsException() {
            var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization.ToLowerInvariant();

            Task Act() => _playerApi.AssertDisplayName(username);
            var error = await Assert.ThrowsAsync<BadRequestException>(Act);
            Assert.Contains("No change required", error.Message); 
        }

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
        public async void ImportValidPlayerResultsWithNoExceptionAndMessageResult() {
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
        public async void SearchPlayerWithInvalidUsernameResultsEmptyCollection() {
            string username = "sghsdfgwe";

            ConnectorCollectionResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.Search(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Empty(response.Data);
        }

        [Fact]
        public async void SearchPlayerWithNoUsernameResultsIntoException() {
            string username = "";
            Task Act() => _playerApi.Search(username);
            await Assert.ThrowsAsync<BadRequestException>(Act);
        }

        [Fact]
        public async void SearchPlayerWithSpecificUsernameResultsIntoCollectionWithOnePlayer() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            ConnectorCollectionResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.Search(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.NotEmpty(response.Data);
            Assert.Contains(response.Data, x => String.Equals(x.Username, username, StringComparison.InvariantCultureIgnoreCase));
            Assert.True(response.Data.Count() == 1);
        }

        [Fact]
        public async void SearchPlayerWithUnspecificUserNameResultsInCollectionWithMultiplePlayers() {
            string username = "iron";

            ConnectorCollectionResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.Search(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.NotEmpty(response.Data);
            Assert.True(response.Data.Count() > 1);
        }

        [Fact]
        public async void SearchPlayerWithUsernameResultsIntoCollectionWithPlayer() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            ConnectorCollectionResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.Search(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.NotEmpty(response.Data);
            Assert.Contains(response.Data, x => String.Equals(x.Username, username, StringComparison.InvariantCultureIgnoreCase));
        }

        [Fact]
        public async void SnapshotByIdAndDayPeriodResultsInMultipleSnapshots() {
            int id = TestConfiguration.ValidPlayerId;

            ConnectorCollectionResponse<Snapshot> response = await _playerApi.Snapshots(id, Period.Day);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async void SnapshotByIdAndMonthPeriodResultsInMultipleSnapshots() {
            int id = TestConfiguration.ValidPlayerId;

            var response = await _playerApi.Snapshots(id, Period.Month);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async void SnapshotByIdAndWeekPeriodResultsInMultipleSnapshots() {
            int id = TestConfiguration.ValidPlayerId;

            var response = await _playerApi.Snapshots(id, Period.Week);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async void SnapshotByIdAndYearPeriodResultsInMultipleSnapshots() {
            int id = TestConfiguration.ValidPlayerId;

            var response = await _playerApi.Snapshots(id, Period.Year);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async void SnapshotByIdResultsInMultipleSnapshots() {
            int id = TestConfiguration.ValidPlayerId;

            ConnectorResponse<Snapshots> response = await _playerApi.Snapshots(id);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.NotEmpty(response.Data.Day);
            Assert.NotEmpty(response.Data.Week);
            Assert.NotEmpty(response.Data.Month);
            Assert.NotEmpty(response.Data.Year);
            Assert.NotEmpty(response.Data.Combined);
            Assert.True(response.Data.Combined.Count == (response.Data.Week.Count + response.Data.Day.Count +
                                                         response.Data.Month.Count + response.Data.Year.Count));
        }

        [Fact]
        public async void SnapshotByUsernameAndDayPeriodResultsInMultipleSnapshots() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            var response = await _playerApi.Snapshots(username, Period.Day);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async void SnapshotByUsernameAndMonthPeriodResultsInMultipleSnapshots() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            var response = await _playerApi.Snapshots(username, Period.Month);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async void SnapshotByUsernameAndWeekPeriodResultsInMultipleSnapshots() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            var response = await _playerApi.Snapshots(username, Period.Week);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async void SnapshotByUsernameAndYearPeriodResultsInMultipleSnapshots() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            var response = await _playerApi.Snapshots(username, Period.Year);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async void SnapshotByUsernameResultsInMultipleSnapshots() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            ConnectorResponse<Snapshots> response = await _playerApi.Snapshots(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.NotEmpty(response.Data.Day);
            Assert.NotEmpty(response.Data.Week);
            Assert.NotEmpty(response.Data.Month);
            Assert.NotEmpty(response.Data.Year);
            Assert.NotEmpty(response.Data.Combined);
            Assert.True(response.Data.Combined.Count == (response.Data.Week.Count + response.Data.Day.Count +
                                                         response.Data.Month.Count + response.Data.Year.Count));
        }

        [Fact]
        public async void TrackingMultipleTimesInShortPeriodResultsInException() {
            string username = TestConfiguration.SecondaryValidPlayerUserName;
            ConnectorResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.Track(username);
            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(username, response.Data.Username, StringComparer.InvariantCultureIgnoreCase);

            Thread.Sleep(10000);
            Task Act() => _playerApi.Track(username);
            await Assert.ThrowsAsync<BadRequestException>(Act);
        }

        [Fact]
        public async void TrackingPlayerByUsernameResultsInPlayer() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;
            ConnectorResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.Track(username);
            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(username, response.Data.Username, StringComparer.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async void TrackingPlayerWithoutUsernameResultsInException() {
            string username = "";
            Task Act() => _playerApi.Track(username);
            await Assert.ThrowsAsync<BadRequestException>(Act);
        }

        [Fact]
        public async void TypeAssertionForHardcoreIronmanIsCorrect() {
            var username = TestConfiguration.ValidHardcoreIronMan;

            var response = _playerApi.AssertPlayerType(username);
            Assert.Equal(PlayerType.HardcoreIronMan, response.Result.Data);

            //Task Act() => _playerApi.AssertPlayerType(username);
            //var error = await Assert.ThrowsAsync<BadRequestException>(Act);

            //Assert.Contains("hardcore", error.WiseOldManMessage, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async void TypeAssertionForHardcoreIronmanIsWrong() {
            var username = TestConfiguration.ValidRegularPlayer;

            var response = _playerApi.AssertPlayerType(username);
            Assert.NotEqual(PlayerType.HardcoreIronMan, response.Result.Data);

        }

        [Fact]
        public async void TypeAssertionForIronmanIsCorrect() {
            var username = TestConfiguration.ValidIronMan;

            var response = _playerApi.AssertPlayerType(username);
            Assert.Equal(PlayerType.IronMan, response.Result.Data);
        }

        [Fact]
        public async void TypeAssertionForIronmanIsWrong() {
            var username = TestConfiguration.ValidRegularPlayer;

            var response = _playerApi.AssertPlayerType(username);
            Assert.NotEqual(PlayerType.IronMan, response.Result.Data);
        }

        [Fact]
        public async void TypeAssertionForRegularPlayerIsCorrect() {
            var username = TestConfiguration.ValidRegularPlayer;

            var response = _playerApi.AssertPlayerType(username);
            Assert.Equal(PlayerType.Regular, response.Result.Data);
        }

        [Fact]
        public async void TypeAssertionForRegularPlayerIsWrong() {
            var username = TestConfiguration.ValidHardcoreIronMan;

            var response = _playerApi.AssertPlayerType(username);
            Assert.NotEqual(PlayerType.Regular, response.Result.Data);
        }

        [Fact]
        public async void TypeAssertionForUltimateIronManIsCorrect() {
            var username = TestConfiguration.ValidUltimateIronMan;

            var response = _playerApi.AssertPlayerType(username);
            Assert.Equal(PlayerType.UltimateIronMan, response.Result.Data);
        }

        [Fact]
        public async void TypeAssertionForUltimateIronManIsWrong() {
            var username = TestConfiguration.ValidRegularPlayer;

            var response = _playerApi.AssertPlayerType(username);
            Assert.NotEqual(PlayerType.UltimateIronMan, response.Result.Data);
        }

        [Fact]
        public async void ViewPlayerByIdResultsIntoPlayerResult() {
            int id = TestConfiguration.ValidPlayerId;

            ConnectorResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.View(id);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(id, response.Data.Id);
        }

        [Fact]
        public void ViewPlayerByInvalidIdResultsInBadRequestException() {
            int id = -1;

            Task Act() => _playerApi.View(id);

            Assert.ThrowsAsync<BadRequestException>(Act);
        }

        [Fact]
        public async void ViewPlayerByUsernameResultsIntoPlayerWithSameUsername() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            ConnectorResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.View(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(username, response.Data.Username, StringComparer.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async void ViewPlayerSnapshotHasAllMetrics() {
            int id = TestConfiguration.ValidPlayerId;

            ConnectorResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.View(id);


            var types = EnumHelper.GetMetricTypes(MetricTypeCategory.All);
            foreach (var type in types) {
                Assert.Contains(type, (IDictionary<MetricType, Metric>) response.Data.LatestSnapshot.AllMetrics);
            }
        }

        [Fact]
        public async void GainedByUserIdResultsInCollection() {
            int id = TestConfiguration.ValidPlayerId;

            var response = await _playerApi.Gained(id);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
            Assert.True(response.Data.Count() == 4);
        }

        [Fact]
        public async void GainedByUsernameResultsInCollection() {
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
        public async void GainedByUserIdAndPeriodResultsInCollection(Period period) {
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
        public async void GainedByUsernameAndPeriodResultsInCollection(Period period) {
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
        public async void RecordsByUsernameAndParametersResultInCollection(string username, MetricType? metric, Period? period) {
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
        public async void RecordsByUserIdAndParametersResultInCollection(int id, MetricType? metric, Period? period) {
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