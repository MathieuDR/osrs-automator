using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models.Output.Exceptions;
using WiseOldManConnector.Models.Requests;
using WiseOldManConnector.Models.WiseOldMan.Enums;
using WiseOldManConnectorTests.Configuration;
using WiseOldManConnectorTests.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace WiseOldManConnectorTests.Connectors;

public class GroupConnectorTests : ConnectorTests {
    private readonly IWiseOldManGroupApi _groupApi;
    private readonly ITestOutputHelper _testOutputHelper;

    public GroupConnectorTests(ApiFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture) {
        _testOutputHelper = testOutputHelper;
        _groupApi = fixture.ServiceProvider.GetService<IWiseOldManGroupApi>();
    }

    [Theory]
    [InlineData(MetricType.Agility, Period.Week)]
    [InlineData(MetricType.Agility, Period.Month)]
    [InlineData(MetricType.Runecrafting, Period.Week)]
    [InlineData(MetricType.AbyssalSire, Period.Week)]
    [InlineData(MetricType.TheCorruptedGauntlet, Period.Month)]
    public async Task GainedLeaderBoardGivesResultOfSameMetricAndPeriod(MetricType metric, Period period) {
        var id = TestConfiguration.ValidGroupId;

        var response = await _groupApi.GainedLeaderboards(id, metric, period);

        Assert.NotNull(response.Data);
        Assert.NotEmpty(response.Data.Members);
    }

    [Theory]
    [InlineData(MetricType.Agility)]
    [InlineData(MetricType.Runecrafting)]
    [InlineData(MetricType.TheCorruptedGauntlet)]
    [InlineData(MetricType.Vetion)]
    public async Task HighscoresGivesResultOfSameMetric(MetricType metric) {
        var id = TestConfiguration.ValidGroupId;

        var response = await _groupApi.Highscores(id, metric);

        Assert.NotNull(response.Data);
        Assert.NotEmpty(response.Data.Members);
    }

  

    private string GetClanName() {
        var id = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        id = Regex.Replace(id, "[/+=]", "");
        var suffix = " .NETCONNECTOR";

        if (id.Length + suffix.Length > 30) {
            id = id.Substring(0, 30 - suffix.Length);
        }

        var result = $"{id}{suffix}";

        _testOutputHelper.WriteLine(result);
        return result;
    }

 

   
    [Fact]
    public async Task CompetitionsForValidGroupWithCompetitionsResultInCompetitions() {
        var id = TestConfiguration.ValidGroupId;

        var response = await _groupApi.Competitions(id);

        Assert.NotEmpty(response.Data);
    }

    [Fact]
    public async Task CompetitionsResultsInValidCompetition() {
        var id = TestConfiguration.ValidGroupId;

        var response = await _groupApi.Competitions(id);
        var compToTest = response.Data.FirstOrDefault();

        Assert.True(compToTest.Id > 0);
        Assert.NotEmpty(compToTest.Title);
        Assert.NotEmpty(compToTest.DurationDescription);
        Assert.Equal(id, compToTest.GroupId.Value);
        Assert.True(compToTest.EndDate > compToTest.StartDate);
        Assert.True(compToTest.ParticipantCount > 0);
    }

  

   
    [Fact]
    public async Task GainedLeaderBoardHasValidDelta() {
        var id = TestConfiguration.ValidGroupId;
        var metric = MetricType.EffectiveHoursPlaying;
        var period = Period.Month;

        var response = await _groupApi.GainedLeaderboards(id, metric, period);
        var delta = response.Data.Members.FirstOrDefault().Delta;

        Assert.True(delta.Gained > 0);
    }

    [Fact]
    public async Task GainedLeaderBoardHasValidDeltaMemberTimes() {
        var id = TestConfiguration.ValidGroupId;
        var metric = MetricType.Slayer;
        var period = Period.Month;

        var response = await _groupApi.GainedLeaderboards(id, metric, period);
        var member = response.Data.Members.FirstOrDefault();

        Assert.True(member.EndTime > member.StartTime);
    }

    [Fact]
    public async Task GainedLeaderBoardHasValidPlayers() {
        var id = TestConfiguration.ValidGroupId;
        var metric = MetricType.Slayer;
        var period = Period.Month;

        var response = await _groupApi.GainedLeaderboards(id, metric, period);
        var player = response.Data.Members.FirstOrDefault().Player;

        Assert.NotEmpty(player.DisplayName);
        Assert.NotEmpty(player.Username);
        Assert.True(player.Id > 0);
        Assert.True(player.UpdatedAt < DateTimeOffset.Now);
        Assert.True(player.RegisteredAt < DateTimeOffset.Now);
    }

   

    [Fact]
    public async Task HighscoresHasValidHighscore() {
        var id = TestConfiguration.ValidGroupId;
        var metric = MetricType.Slayer;

        var response = await _groupApi.Highscores(id, metric);
        var highscore = response.Data.Members.FirstOrDefault();

        Assert.Equal(metric, highscore.MetricType);
        Assert.True(highscore.Metric.Level > 1 && highscore.Metric.Level < 100);
        Assert.True(highscore.Metric.Rank > 0);
        Assert.True(highscore.Metric.Value >= 36690434);
    }

    [Fact]
    public async Task HighscoresHasValidPlayers() {
        var id = TestConfiguration.ValidGroupId;
        var metric = MetricType.Slayer;

        var response = await _groupApi.Highscores(id, metric);
        var player = response.Data.Members.FirstOrDefault().Player;

        Assert.NotEmpty(player.DisplayName);
        Assert.NotEmpty(player.Username);
        Assert.True(player.Id > 0);
        Assert.True(player.UpdatedAt < DateTimeOffset.Now);
        Assert.True(player.RegisteredAt < DateTimeOffset.Now);
    }

    [Fact]
    public async Task HighscoresHasValidCategory() {
        var id = TestConfiguration.ValidGroupId;
        var metric = MetricType.Slayer;

        var response = await _groupApi.Highscores(id, metric);
        var member = response.Data.Members.FirstOrDefault();


        Assert.Equal(metric, member.MetricType);
        Assert.Equal(metric, response.Data.MetricType);
    }

    [Fact]
    public async Task MembersFromGroupResultInValidPlayers() {
        var id = TestConfiguration.ValidGroupId;

        var response = await _groupApi.GetMembers(id);
        var player = response.Data.FirstOrDefault();

        var flags = response.Data.Select(x => x.Country).Distinct();

        Assert.NotEmpty(player.DisplayName);
        Assert.NotEmpty(player.Username);
        Assert.True(player.Id > 0);
        Assert.True(player.OverallExperience > 1000);
        Assert.NotNull(player.Role);
        Assert.True(player.UpdatedAt < DateTimeOffset.Now);
        Assert.True(player.RegisteredAt < DateTimeOffset.Now);
        Assert.True(flags.Count() >= 3);
    }

    [Fact]
    public async Task MembersFromValidGroupResultsInMultipleMembers() {
        var id = TestConfiguration.ValidGroupId;

        var response = await _groupApi.GetMembers(id);

        Assert.NotNull(response);
        Assert.NotEmpty(response.Data);
    }

    [Fact]
    public async Task RecentAchievementsHasValidAchievements() {
        var id = TestConfiguration.ValidGroupId;

        var response = await _groupApi.RecentAchievements(id);
        var achievement = response.Data.FirstOrDefault();

        Assert.NotEmpty(achievement.Title);
        Assert.NotNull(achievement.AchievedAt);
        Assert.NotEqual(DateTime.MinValue, achievement.AchievedAt);
        Assert.True(achievement.PlayerId > 0);
        Assert.True(achievement.Threshold > 0);
    }

   [Fact]
    public async Task ViewGroupResultsInValidGroup() {
        var id = TestConfiguration.ValidGroupId;

        var response = await _groupApi.View(id);

        Assert.Equal(id, response.Data.Id);
        Assert.NotEmpty(response.Data.Name);
        Assert.NotEmpty(response.Data.ClanChat);
        Assert.True(response.Data.MemberCount > 1);
        Assert.True(response.Data.Verified);
        Assert.True(response.Data.Score > 1);
    }

    [Fact]
    public async Task ViewGroupWithInvalidIdResultsInError() {
        var id = TestConfiguration.InvalidGroupId;

        Task Act() {
            return _groupApi.View(id);
        }

        var exception = await Assert.ThrowsAsync<BadRequestException>(Act);
        Assert.NotEmpty(exception.Message);
    }

    [Fact]
    public async Task ViewGroupWithValidIdResultsInGroupResult() {
        var id = TestConfiguration.ValidGroupId;

        var response = await _groupApi.View(id);

        Assert.NotNull(response);
        Assert.NotNull(response.Data);
        Assert.Equal(id, response.Data.Id);
    }
}
