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
using Xunit.Abstractions;
using Record = WiseOldManConnector.Models.Output.Record;

namespace WiseOldManConnectorTests.Connectors;

public class PlayerConnectorTests : ConnectorTests {
    private readonly IWiseOldManPlayerApi _playerApi;
    private readonly ITestOutputHelper _testOutputHelper;

    public PlayerConnectorTests(ApiFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture) {
        _testOutputHelper = testOutputHelper;
        _playerApi = fixture.ServiceProvider.GetService<IWiseOldManPlayerApi>();
    }

   
    [Fact]
    public async Task SearchPlayerWithInvalidUsernameResultsEmptyCollection() {
        var username = "sghsdfgwe";

        var response = await _playerApi.Search(username);

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.Empty(response.Data);
    }

    [Fact]
    public async Task SearchPlayerWithNoUsernameResultsIntoException() {
        var username = "";

        Task Act() {
            return _playerApi.Search(username);
        }

        await Assert.ThrowsAsync<BadRequestException>(Act);
    }

    [Fact]
    public async Task SearchPlayerWithSpecificUsernameResultsIntoCollectionWithOnePlayer() {
        var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

        var response = await _playerApi.Search(username);

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.NotEmpty(response.Data);
        Assert.Contains(response.Data, x => string.Equals(x.Username, username, StringComparison.InvariantCultureIgnoreCase));
        Assert.True(response.Data.Count() == 1);
    }

    [Fact]
    public async Task SearchPlayerResultsInValidPlayer() {
        var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

        var response = await _playerApi.Search(username);


        var player = response.Data.FirstOrDefault();

        Assert.NotEmpty(player.DisplayName);
        Assert.NotEmpty(player.Username);
        Assert.True(player.Id > 0);
        Assert.Null(player.LatestSnapshot);
        Assert.Null(player.Role);
        Assert.NotEqual(0, player.OverallExperience);
        Assert.True(player.UpdatedAt < DateTimeOffset.Now);
        Assert.True(player.RegisteredAt < DateTimeOffset.Now);
    }

    [Fact]
    public async Task SearchPlayerWithUnspecificUserNameResultsInCollectionWithMultiplePlayers() {
        var username = "iron";

        var response = await _playerApi.Search(username);

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.NotEmpty(response.Data);
        Assert.True(response.Data.Count() > 1);
    }

    [Fact]
    public async Task SearchPlayerWithUsernameResultsIntoCollectionWithPlayer() {
        var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;

        var response = await _playerApi.Search(username);

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.NotEmpty(response.Data);
        Assert.Contains(response.Data, x => string.Equals(x.Username, username, StringComparison.InvariantCultureIgnoreCase));
    }

   

    [Fact]
    public async Task TrackingMultipleTimesInShortPeriodResultsInException() {
        var username = TestConfiguration.SecondaryValidPlayerUserName;
        var response = await _playerApi.Track(username);
        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.Equal(username, response.Data.Username, StringComparer.InvariantCultureIgnoreCase);

        Thread.Sleep(10000);

        Task Act() {
            return _playerApi.Track(username);
        }

        await Assert.ThrowsAsync<BadRequestException>(Act);
    }

    [Fact]
    public async Task TrackingPlayerByUsernameResultsInPlayer() {
        var username = TestConfiguration.ValidPlayerUsernameWithValidCapatilization;
        var response = await _playerApi.Track(username);
        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.Equal(username, response.Data.Username, StringComparer.InvariantCultureIgnoreCase);
    }

    [Fact]
    public async Task TrackingPlayerWithoutUsernameResultsInException() {
        var username = "";

        Task Act() {
            return _playerApi.Track(username);
        }

        await Assert.ThrowsAsync<BadRequestException>(Act);
    }


    [Fact]
    public async Task ViewPlayerByIdResultsIntoPlayerResult() {
        var id = TestConfiguration.ValidPlayerId;

        var response = await _playerApi.View(id);

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.Equal(id, response.Data.Id);
    }

    [Fact]
    public async Task ViewRegularPlayerResultsInCorrectType() {
        var id = TestConfiguration.ValidPlayerId;

        var response = await _playerApi.View(id);

        Assert.Equal(PlayerType.Regular, response.Data.Type);
    }

    [Fact]
    public async Task ViewIronManPlayerResultsInCorrectType() {
        var id = TestConfiguration.ValidIronManId;

        var response = await _playerApi.View(id);

        Assert.Equal(PlayerType.IronMan, response.Data.Type);
    }

    [Fact]
    public async Task ViewMainPlayerResultsInCorrectMode() {
        var id = TestConfiguration.ValidPlayerId;

        var response = await _playerApi.View(id);

        Assert.Equal(PlayerBuild.Main, response.Data.Build);
    }

    [Fact]
    public async Task ViewPlayerByIdResultsInValidPlayer() {
        var id = TestConfiguration.ValidPlayerId;

        var response = await _playerApi.View(id);

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
        var id = -1;

        Task Act() {
            return _playerApi.View(id);
        }

        Assert.ThrowsAsync<BadRequestException>(Act);
    }

    [Fact]
    public async Task ViewPlayerSnapshotHasAllMetrics() {
        var id = TestConfiguration.ValidPlayerId;

        var response = await _playerApi.View(id);

        var types = MetricTypeCategory.Queryable.GetMetricTypes();
        var hasHours = false;

        foreach (var type in types) {
            _testOutputHelper.WriteLine($"metric: {type}");
            Assert.Contains(type, (IDictionary<MetricType, Metric>)response.Data.LatestSnapshot.AllMetrics);

            var metric = response.Data.LatestSnapshot.AllMetrics[type];

            if (metric.Rank != -1) {
                Assert.True(metric.Rank > 0);
                Assert.True(metric.Value > 0);

                if (metric.EffectiveHours > 0) {
                    hasHours = true;
                }
            } else {
                _testOutputHelper.WriteLine("No rank");
            }
        }

        Assert.True(hasHours);
    }
}
