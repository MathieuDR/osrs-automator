using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;
using WiseOldManConnectorTests.Configuration;
using WiseOldManConnectorTests.Fixtures;
using Xunit;

namespace WiseOldManConnectorTests.Connectors.Player
{
    public class PlayerConnectorTests:IClassFixture<APIFixture>
    {
        private ServiceProvider _serviceProvide;
        private IWiseOldManPlayerApi _playerApi;

        public PlayerConnectorTests(APIFixture fixture)
        {
            _serviceProvide = fixture.ServiceProvider;
            _playerApi = fixture.ServiceProvider.GetService<IWiseOldManPlayerApi>();
        }

        [Fact]
        public async void GetPlayerByValidUsername() {
            string username = TestConfiguration.ValidPlayerUsername;

            ConnectorResponse<WiseOldManConnector.Models.Output.Player> response = await _playerApi.View(username);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(username.ToLowerInvariant(), response.Data.Username.ToLowerInvariant());

            var names = Enum.GetNames(typeof(MetricType));

            foreach (string name in names) {
                MetricType type = (MetricType)Enum.Parse(typeof(MetricType), name);
                Assert.Contains(type, (IDictionary<MetricType, Metric>)response.Data.LatestSnapshot.AllMetrics);
            }

            Assert.Equal(Enum.GetNames(typeof(MetricType)).Length, response.Data.LatestSnapshot.AllMetrics.Count);
        }
    }
}
