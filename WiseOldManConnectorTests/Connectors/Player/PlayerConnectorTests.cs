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
            string username = TestConfiguration.ValidPlayerUsername;

            ConnectorResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.View(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(username, response.Data.Username, StringComparer.InvariantCultureIgnoreCase);
        }

        #endregion

        #region search
        [Fact]
        public async void SearchPlayerWithValidName() {
            string username = TestConfiguration.ValidPlayerUsername;

            ConnectorCollectionResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.Search(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.NotEmpty(response.Data);
            Assert.Contains(response.Data, x => String.Equals(x.Username, username, StringComparison.InvariantCultureIgnoreCase));
        }

        [Fact]
        public async void SearchPlayerWithSpecificUserNameResultsOne() {
            string username = TestConfiguration.ValidPlayerUsername;

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
            string username = TestConfiguration.ValidPlayerUsername;
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
    }
}