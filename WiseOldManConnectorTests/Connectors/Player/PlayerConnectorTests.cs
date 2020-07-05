using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.Output.Exceptions;
using WiseOldManConnector.Models.WiseOldMan.Enums;
using WiseOldManConnectorTests.Configuration;
using WiseOldManConnectorTests.Fixtures;
using Xunit;

namespace WiseOldManConnectorTests.Connectors.Player {
    public class PlayerConnectorTests : ConnectorTests {
        public PlayerConnectorTests(APIFixture fixture) : base(fixture) {
            _playerApi = fixture.ServiceProvider.GetService<IWiseOldManPlayerApi>();
        }

        private readonly IWiseOldManPlayerApi _playerApi;

        #region view

        [Fact]
        public void ViewPlayerByInvalidId() {
            int id = -1;

            Task act() => _playerApi.View(id);

            Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async void ViewPlayerByValidId() {
            int id = TestConfiguration.ValidPlayerId;

            ConnectorResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.View(id);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(id, response.Data.Id);
        }

        [Fact]
        public async void ViewPlayerHasAllMetrics() {
            int id = TestConfiguration.ValidPlayerId;

            ConnectorResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.View(id);


            var names = Enum.GetNames(typeof(MetricType));
            foreach (string name in names) {
                MetricType type = (MetricType) Enum.Parse(typeof(MetricType), name);
                Assert.Contains(type, (IDictionary<MetricType, Metric>) response.Data.LatestSnapshot.AllMetrics);
            }
        }


        [Fact]
        public async void ViewPlayerByValidUsername() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            ConnectorResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.View(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(username, response.Data.Username, StringComparer.InvariantCultureIgnoreCase);
        }

        #endregion

        #region search

        [Fact]
        public async void SearchPlayerWithValidName() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            ConnectorCollectionResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.Search(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.NotEmpty(response.Data);
            Assert.Contains(response.Data, x => String.Equals(x.Username, username, StringComparison.InvariantCultureIgnoreCase));
        }

        [Fact]
        public async void SearchPlayerWithSpecificUserNameResultsOne() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            ConnectorCollectionResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.Search(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.NotEmpty(response.Data);
            Assert.Contains(response.Data, x => String.Equals(x.Username, username, StringComparison.InvariantCultureIgnoreCase));
            Assert.True(response.Data.Count() == 1);
        }

        [Fact]
        public async void SearchPlayerWithUnspecificUserNameResultsInMultiple() {
            string username = "iron";

            ConnectorCollectionResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.Search(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.NotEmpty(response.Data);
            Assert.True(response.Data.Count() > 1);
        }

        [Fact]
        public async void SearchPlayerResultsInEmptyCollection() {
            string username = "sghsdfgwe";

            ConnectorCollectionResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.Search(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Empty(response.Data);
        }


        [Fact]
        public async void SearchPlayerWithNoUsernameResultsInException() {
            string username = "";
            Task act() => _playerApi.Search(username);
            Assert.ThrowsAsync<BadRequestException>(act);
        }

        #endregion

        #region tracking

        [Fact]
        public async void TrackValidPlayerResultsInPlayer() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;
            ConnectorResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.Track(username);
            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(username, response.Data.Username, StringComparer.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async void TackingMultipleTimesResultInError() {
            string username = TestConfiguration.SecondaryValidPlayerUserName;
            ConnectorResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.Track(username);
            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(username, response.Data.Username, StringComparer.InvariantCultureIgnoreCase);

            Thread.Sleep(10000);
            Task act() => _playerApi.Track(username);
            Assert.ThrowsAsync<BadRequestException>(act);
        }

        [Fact]
        public async void TrackingPlayerWithNoUsernameResultsInException() {
            string username = "";
            Task act() => _playerApi.Track(username);
            Assert.ThrowsAsync<BadRequestException>(act);
        }

        #endregion

        #region import

        [Fact]
        public async void ImportValidPlayerDoesNotFail() {
            string username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;
            try {
                ConnectorResponse<MessageResponse> response = await _playerApi.Import(username);
                Assert.NotNull(response);
                Assert.NotNull(response.Data);
                Assert.NotEmpty(response.Data.Message);
            } catch (BadRequestException e) {
                if (!e.WiseOldManMessage.Contains("Failed to load history from CML.")) {
                    // If it contains above string, CML is unresponsive and cannot test.
                    throw;
                }
            }
        }

        [Fact]
        public async void ImportingInvalidPlayerResultsInError() {
            string username = "";
            Task act() => _playerApi.Import(username);
            Assert.ThrowsAsync<BadRequestException>(act);
        }

        #endregion

        #region competitions

        [Fact]
        public async void GettingCompetitionsFromValidUsernameResultsInMultipleCompetitions() {
            var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            ConnectorCollectionResponse<Competition> response = await _playerApi.Competitions(username);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async void GettingCompetitionsFromInvalidUsernameResultsInErrorWithValidMessage() {
            var username = TestConfiguration.InvalidUser;

            Task act() => _playerApi.Competitions(username);
            var exception = await Assert.ThrowsAnyAsync<BadRequestException>(act);

            Assert.NotNull(exception);
            Assert.NotEmpty(exception.WiseOldManMessage);
        }

        [Fact]
        public async void GettingCompetitionsFromValidIdResultsInMultipleCompetitions() {
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
            ;
            Assert.True(competition.StartDate < competition.EndDate);
            Assert.NotEmpty(competition.Duration);
            Assert.True(competition.Participants > 1);
        }

        #endregion

        #region asserting

        [Fact]
        public async void RegularPlayerCorrectlyAsserted() {
            var username = TestConfiguration.ValidRegularPlayer;

            Task act() => _playerApi.AssertPlayerType(username);
            var error = await Assert.ThrowsAsync<BadRequestException>(act);

            Assert.Contains("regular", error.WiseOldManMessage, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async void IronManPlayerCorrectlyAsserted() {
            var username = TestConfiguration.ValidIronMan;

            Task act() => _playerApi.AssertPlayerType(username);
            var error = await Assert.ThrowsAsync<BadRequestException>(act);

            Assert.Contains("ironman", error.WiseOldManMessage, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async void HardcoreIronManPlayerCorrectlyAsserted() {
            var username = TestConfiguration.ValidHardcoreIronMan;

            Task act() => _playerApi.AssertPlayerType(username);
            var error = await Assert.ThrowsAsync<BadRequestException>(act);

            Assert.Contains("hardcore", error.WiseOldManMessage, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async void UltimateIronManPlayerCorrectlyAsserted() {
            var username = TestConfiguration.ValidUltimateIronMan;

            Task act() => _playerApi.AssertPlayerType(username);
            var error = await Assert.ThrowsAsync<BadRequestException>(act);

            Assert.Contains("ultimate", error.WiseOldManMessage, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async void RegularPlayerIncorrectlyAsserted() {
            var username = TestConfiguration.ValidHardcoreIronMan;

            Task act() => _playerApi.AssertPlayerType(username);
            var error = await Assert.ThrowsAsync<BadRequestException>(act);

            Assert.DoesNotContain("regular", error.WiseOldManMessage, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async void IronManPlayerIncorrectlyAsserted() {
            var username = TestConfiguration.ValidRegularPlayer;

            Task act() => _playerApi.AssertPlayerType(username);
            var error = await Assert.ThrowsAsync<BadRequestException>(act);

            Assert.DoesNotContain("ironman", error.WiseOldManMessage, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async void HardcoreIronManPlayerIncorrectlyAsserted() {
            var username = TestConfiguration.ValidRegularPlayer;

            Task act() => _playerApi.AssertPlayerType(username);
            var error = await Assert.ThrowsAsync<BadRequestException>(act);

            Assert.DoesNotContain("hardcore", error.WiseOldManMessage, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async void UltimateIronManPlayerIncorrectlyAsserted() {
            var username = TestConfiguration.ValidRegularPlayer;

            Task act() => _playerApi.AssertPlayerType(username);
            var error = await Assert.ThrowsAsync<BadRequestException>(act);

            Assert.DoesNotContain("ultimate", error.WiseOldManMessage, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion

        #region AssertDisplayName

        [Fact]
        public async void DisplayNameAssertionThrowsErrorOnEmptyUserName() {
            var username = "";

            Task act() => _playerApi.AssertDisplayName(username);
            var error = await Assert.ThrowsAsync<BadRequestException>(act);
        }

        // Cannot Test
        //[Fact]
        //public async void DisplayNameAssertionIsCorrectlyCapatalized() {
        //    var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization.ToLowerInvariant();

        //    var response = await _playerApi.AssertDisplayName(username);
        //    Assert.Equal(TestConfiguration.ValidPlayerUsernameWithValidCapatilization, response.Data);
        //}

        [Fact]
        public async void DisplayNameAssertionThrowsErrorOnAlreadyCorrectName() {
            var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization.ToLowerInvariant();

            Task act() => _playerApi.AssertDisplayName(username);
            var error = await Assert.ThrowsAsync<BadRequestException>(act);
            Assert.Contains("No change required", error.Message);
        }

        #endregion

        #region achievements

        [Fact]
        public async void AchievementForValidUserResultsInAchievements() {
            var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            var response = await _playerApi.Achievements(username);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async void AchievementForValidUserResultsInAchievementsWithMissings() {
            var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

            var response = await _playerApi.Achievements(username, true);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
            Assert.True(Enum.GetValues(typeof(MetricType)).Length <= response.Data.Count());
        }

        [Fact]
        public async void AchievementForValidAccomplishedUserResultsInMultipleAchievements() {
            var username = TestConfiguration.ValidAccomplishedPlayer;

            var response = await _playerApi.Achievements(username);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
            Assert.True(response.Data.Count() > 1);
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

        #endregion
    }
}