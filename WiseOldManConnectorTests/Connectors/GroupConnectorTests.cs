using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.Output.Exceptions;
using WiseOldManConnector.Models.Requests;
using WiseOldManConnector.Models.WiseOldMan.Enums;
using WiseOldManConnectorTests.Configuration;
using WiseOldManConnectorTests.Fixtures;
using Xunit;
using Record = WiseOldManConnector.Models.Output.Record;

namespace WiseOldManConnectorTests.Connectors {
    public class GroupConnectorTests : ConnectorTests {
        public GroupConnectorTests(APIFixture fixture) : base(fixture) {
            _groupApi = fixture.ServiceProvider.GetService<IWiseOldManGroupApi>();
        }

        private readonly IWiseOldManGroupApi _groupApi;

        [Theory]
        [InlineData(new object[] {MetricType.Agility, Period.Week})]
        [InlineData(new object[] {MetricType.Agility, Period.Month})]
        [InlineData(new object[] {MetricType.Runecrafting, Period.Week})]
        [InlineData(new object[] {MetricType.AbyssalSire, Period.Week})]
        [InlineData(new object[] {MetricType.TheCorruptedGauntlet, Period.Month})]
        public async Task GainedLeaderBoardGivesResultOfSameMetricAndPeriod(MetricType metric, Period period) {
            int id = TestConfiguration.ValidGroupId;

            var response = await _groupApi.GainedLeaderboards(id, metric, period);

            Assert.NotNull(response.Data);
            Assert.NotEmpty(response.Data.Members);
        }

        [Theory]
        [InlineData(new object[] {MetricType.Agility})]
        [InlineData(new object[] {MetricType.Runecrafting})]
        [InlineData(new object[] {MetricType.TheCorruptedGauntlet})]
        [InlineData(new object[] {MetricType.Vetion})]
        public async Task HighscoresGivesResultOfSameMetric(MetricType metric) {
            int id = TestConfiguration.ValidGroupId;

            var response = await _groupApi.Highscores(id, metric);

            Assert.NotNull(response.Data);
            Assert.NotEmpty(response.Data.Members);
        }

        [Theory]
        [InlineData(new object[] {MetricType.Agility, Period.Week})]
        [InlineData(new object[] {MetricType.Agility, Period.Month})]
        [InlineData(new object[] {MetricType.Runecrafting, Period.Week})]
        [InlineData(new object[] {MetricType.AbyssalSire, Period.Week})]
        [InlineData(new object[] {MetricType.TheCorruptedGauntlet, Period.Month})]
        public async Task RecordLeaderBoardGivesResultOfSameMetricAndPeriod(MetricType metric, Period period) {
            int id = TestConfiguration.ValidGroupId;

            var response = await _groupApi.RecordLeaderboards(id, metric, period);

            Assert.NotNull(response.Data);
            Assert.NotEmpty(response.Data.Members);
        }

        private string GetClanName() {
            string id = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            id = Regex.Replace(id, "[/+=]", "");
            string suffix = " .NETCONNECTOR";

            if (id.Length + suffix.Length > 30) {
                id = id.Substring(0, 30 - suffix.Length);
            }

            return $"{id}{suffix}";
        }

        private async Task DeleteGroup(int id, string verificationCode) {
            var response = await _groupApi.Delete(id, verificationCode);
            if (response.Data.IsError) {
                throw new Exception();
            }
        }

        [Fact]
        public async Task AddingMembersHasNewMembersTask() {
            var cname = GetClanName();
            var cc = "MyClanChat";
            var createRequest = new CreateGroupRequest(cname, cc, new List<MemberRequest>() {
                new MemberRequest() {Name = "Den Badjas", Role = GroupRole.Leader}
            });


            var createResponse = await _groupApi.Create(createRequest);


            var request = new List<MemberRequest>() {
                new MemberRequest() {
                    Name = "WouterPils", Role = GroupRole.Member
                },
                new MemberRequest() {
                    Name = "ErkendRserke", Role = GroupRole.Leader
                }
            };
            
            try {
                var response = await _groupApi.AddMembers(createResponse.Data.Id, createResponse.Data.VerificationCode, request);
                Assert.NotNull(response.Data);
                Assert.Equal(request.Count() + createRequest.Members.Count(), response.Data.Members.Count);

                foreach (MemberRequest member in request) {
                    Assert.Contains(response.Data.Members,
                        player => player.DisplayName.Equals(member.Name, StringComparison.InvariantCultureIgnoreCase));

                    Assert.Equal(member.Role,
                        response.Data.Members
                            .Where(x => x.DisplayName.Equals(member.Name, StringComparison.InvariantCultureIgnoreCase))
                            .FirstOrDefault().Role);
                }
            }
            finally {
                await DeleteGroup(createResponse.Data.Id, createResponse.Data.VerificationCode);
            }
        }

        [Fact]
        public async Task CompetitionsForValidGroupWithCompetitionsResultInCompetitions() {
            int id = TestConfiguration.ValidGroupId;

            var response = await _groupApi.Competitions(id);

            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async Task CompetitionsResultsInValidCompetition() {
            int id = TestConfiguration.ValidGroupId;

            var response = await _groupApi.Competitions(id);
            var compToTest = response.Data.FirstOrDefault();

            Assert.True(compToTest.Id > 0);
            Assert.NotEmpty(compToTest.Title);
            Assert.NotEmpty(compToTest.Duration);
            Assert.Equal(id, compToTest.GroupId.Value);
            Assert.True(compToTest.EndDate > compToTest.StartDate);
            Assert.True(compToTest.ParticipantCount > 0);
        }


        [Fact]
        public async Task CreateGroupResultsInGroupWithCorrectParams() {
            var cname = GetClanName();
            var cc = "MyClanChat";
            var request = new CreateGroupRequest(cname, cc, new List<MemberRequest>() {
                new MemberRequest() {Name = "ErkendRserke"},
                new MemberRequest() {Name = "Den Badjas", Role = GroupRole.Leader}
            });

            var response = await _groupApi.Create(request);

            try {
                Assert.NotNull(response.Data);
                Assert.Equal(request.Members.Count(), response.Data.Members.Count);
                Assert.Equal(request.ClanChat, response.Data.ClanChat);
                Assert.Equal(request.Name, response.Data.Name);

                foreach (MemberRequest member in request.Members) {
                    Assert.Contains(response.Data.Members,
                        player => player.DisplayName.Equals(member.Name, StringComparison.InvariantCultureIgnoreCase));
                    Assert.Equal(member.Role,
                        response.Data.Members
                            .Where(x => x.DisplayName.Equals(member.Name, StringComparison.InvariantCultureIgnoreCase))
                            .FirstOrDefault().Role);
                }
            }
            finally {
                await DeleteGroup(response.Data.Id, response.Data.VerificationCode);
            }
        }

        [Fact]
        public async Task CreateGroupResultsInGroupWithVerificationCode() {
            var cname = GetClanName();
            var cc = "MyClanChat";
            var request = new CreateGroupRequest(cname, cc, new List<MemberRequest>() {
                new MemberRequest() {Name = "ErkendRserke"},
                new MemberRequest() {Name = "Den Badjas", Role = GroupRole.Leader}
            });

            var response = await _groupApi.Create(request);

            try {
                Assert.NotNull(response.Data);
                Assert.NotEmpty(response.Data.VerificationCode);
            }
            finally {
                await DeleteGroup(response.Data.Id, response.Data.VerificationCode);
            }
        }

        [Fact]
        public async Task DeleteGroupResultsInDeletedGroupMessage() {
            var request = new CreateGroupRequest(GetClanName(), "myChat", new List<MemberRequest>() {
                new MemberRequest() {Name = "ErkendRserke"},
                new MemberRequest() {Name = "Den Badjas", Role = GroupRole.Leader}
            });

            var createdGroup = await _groupApi.Create(request);
            var response = await _groupApi.Delete(createdGroup.Data.Id, createdGroup.Data.VerificationCode);

            Assert.NotNull(response.Data);
            Assert.Contains("Successfully", response.Data.Message);
            Assert.Contains(createdGroup.Data.Id.ToString(), response.Data.Message);
        }

        [Fact]
        public async Task EditedGroupHasEditedParameters() {
            var cname = GetClanName();
            var cc = "MyClanChat";
            var createRequest = new CreateGroupRequest(cname, cc, new List<MemberRequest>() {
                new MemberRequest() {Name = "ErkendRserke"},
                new MemberRequest() {Name = "Den Badjas", Role = GroupRole.Leader}
            });


            var createResponse = await _groupApi.Create(createRequest);


            var request = new EditGroupRequest(createResponse.Data.VerificationCode, GetClanName(), "MySecondChat",
                new[] {new MemberRequest() {Name = "WouterPils"}});

            var response = await _groupApi.Edit(createResponse.Data.Id, request);

            try {
                Assert.NotNull(response.Data);
                Assert.Equal(request.Members.Count(), response.Data.Members.Count);
                Assert.Equal(request.ClanChat, response.Data.ClanChat);
                Assert.Equal(request.Name, response.Data.Name);

                foreach (MemberRequest member in request.Members) {
                    Assert.Contains(response.Data.Members,
                        player => player.DisplayName.Equals(member.Name, StringComparison.InvariantCultureIgnoreCase));
                    Assert.Equal(member.Role,
                        response.Data.Members
                            .Where(x => x.DisplayName.Equals(member.Name, StringComparison.InvariantCultureIgnoreCase))
                            .FirstOrDefault().Role);
                }
            }
            finally {
                await DeleteGroup(response.Data.Id, createResponse.Data.VerificationCode);
            }
        }

        [Fact]
        public async Task GainedLeaderBoardGivesCorrectPagingSize() {
            int id = TestConfiguration.ValidGroupId;
            var metric = MetricType.Slayer;
            var period = Period.Month;
            var size = 7;
            var offset = 1;

            var response = await _groupApi.GainedLeaderboards(id, metric, period, size, offset);

            Assert.Equal(size, response.Data.PageSize);
            Assert.Equal(offset, response.Data.Page);
            Assert.Equal(size, response.Data.Members.Count);
        }

        [Fact]
        public async Task GainedLeaderBoardHasValidDelta() {
            int id = TestConfiguration.ValidGroupId;
            var metric = MetricType.Slayer;
            var period = Period.Month;

            var response = await _groupApi.GainedLeaderboards(id, metric, period);
            var delta = response.Data.Members.FirstOrDefault().Delta;

            Assert.True(delta.Gained > 0);
        }

        [Fact]
        public async Task GainedLeaderBoardHasValidDeltaMemberTimes() {
            int id = TestConfiguration.ValidGroupId;
            var metric = MetricType.Slayer;
            var period = Period.Month;

            var response = await _groupApi.GainedLeaderboards(id, metric, period);
            var member = response.Data.Members.FirstOrDefault();

            Assert.True(member.EndTime > member.StartTime);
        }

        [Fact]
        public async Task GainedLeaderBoardHasValidPlayers() {
            int id = TestConfiguration.ValidGroupId;
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
        public async Task HighscoresGivesCorrectPagingSize() {
            int id = TestConfiguration.ValidGroupId;
            var metric = MetricType.Slayer;
            var size = 7;
            var offset = 1;

            var response = await _groupApi.Highscores(id, metric, size, offset);

            Assert.Equal(size, response.Data.PageSize);
            Assert.Equal(offset, response.Data.Page);
            Assert.Equal(size, response.Data.Members.Count);
        }

        [Fact]
        public async Task HighscoresHasValidHighscore() {
            int id = TestConfiguration.ValidGroupId;
            var metric = MetricType.Slayer;

            var response = await _groupApi.Highscores(id, metric);
            var highscore = response.Data.Members.FirstOrDefault();

            Assert.Equal(metric, highscore.MetricType);
            Assert.True(highscore.Metric.Level > 1 && highscore.Metric.Level < 100);
            Assert.True(highscore.Metric.Rank > 0);
            Assert.True(highscore.Metric.Experience > 1);
        }

        [Fact]
        public async Task HighscoresHasValidPlayers() {
            int id = TestConfiguration.ValidGroupId;
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
        public async Task MembersFromGroupResultInValidPlayers() {
            int id = TestConfiguration.ValidGroupId;

            var response = await _groupApi.GetMembers(id);
            var player = response.Data.FirstOrDefault();

            Assert.NotEmpty(player.DisplayName);
            Assert.NotEmpty(player.Username);
            Assert.True(player.Id > 0);
            Assert.True(player.OverallExperience > 1000);
            Assert.NotNull(player.Role);
            Assert.True(player.UpdatedAt < DateTimeOffset.Now);
            Assert.True(player.RegisteredAt < DateTimeOffset.Now);
        }

        [Fact]
        public async Task MembersFromValidGroupResultsInMultipleMembers() {
            int id = TestConfiguration.ValidGroupId;

            var response = await _groupApi.GetMembers(id);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async Task MonthlyTopPlayerResultsInValidPlayer() {
            int id = TestConfiguration.ValidGroupId;

            var response = await _groupApi.GetMonthTopMember(id);
            var player = response.Data.Player;

            Assert.NotEmpty(player.DisplayName);
            Assert.NotEmpty(player.Username);
            Assert.True(player.Id > 0);
            Assert.True(player.UpdatedAt < DateTimeOffset.Now);
            Assert.True(player.RegisteredAt < DateTimeOffset.Now);
            Assert.True(response.Data.Delta.Gained > 0);

            Assert.True(response.Data.EndTime > response.Data.StartTime);
        }

        [Fact]
        public async Task MonthlyTopPlayOfValidGroupResultsInPlayer() {
            int id = TestConfiguration.ValidGroupId;

            var response = await _groupApi.GetMonthTopMember(id);

            Assert.NotNull(response.Data);
            Assert.NotNull(response.Data.Player);
            Assert.NotNull(response.Data.Delta);
        }

        [Fact]
        public async Task RecentAchievementHasAchievements() {
            int id = TestConfiguration.ValidGroupId;
            var size = 7;
            var offset = 1;

            var response = await _groupApi.RecentAchievements(id, size, offset);

            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async Task RecentAchievementHasCorrectPaging() {
            int id = TestConfiguration.ValidGroupId;
            var size = 7;
            var offset = 1;

            var response = await _groupApi.RecentAchievements(id, size, offset);

            Assert.Equal(size, response.Data.Count());
        }

        [Fact]
        public async Task RecentAchievementsHasValidAchievements() {
            int id = TestConfiguration.ValidGroupId;

            var response = await _groupApi.RecentAchievements(id);
            var achievement = response.Data.FirstOrDefault();

            Assert.NotEmpty(achievement.Title);
            Assert.NotNull(achievement.AchievedAt);
            Assert.NotEqual(DateTime.MinValue, achievement.AchievedAt);
            Assert.True(achievement.PlayerId > 0);
            Assert.True(achievement.Threshold > 0);
        }

        [Fact]
        public async Task RecordLeaderBoardGivesCorrectPagingSize() {
            int id = TestConfiguration.ValidGroupId;
            var metric = MetricType.Slayer;
            var period = Period.Month;
            var size = 7;
            var offset = 1;

            var response = await _groupApi.RecordLeaderboards(id, metric, period, size, offset);

            Assert.Equal(size, response.Data.PageSize);
            Assert.Equal(offset, response.Data.Page);
            Assert.Equal(size, response.Data.Members.Count);
        }

        [Fact]
        public async Task RecordLeaderBoardHasValidDeltaRecordTimes() {
            int id = TestConfiguration.ValidGroupId;
            var metric = MetricType.Slayer;
            var period = Period.Month;

            var response = await _groupApi.RecordLeaderboards(id, metric, period);
            var record = response.Data.Members.FirstOrDefault();

            Assert.True(DateTimeOffset.Now > record.UpdateDateTime);
        }

        [Fact]
        public async Task RecordLeaderBoardHasValidPlayers() {
            int id = TestConfiguration.ValidGroupId;
            var metric = MetricType.Slayer;
            var period = Period.Month;

            var response = await _groupApi.RecordLeaderboards(id, metric, period);
            var player = response.Data.Members.FirstOrDefault().Player;

            Assert.NotEmpty(player.DisplayName);
            Assert.NotEmpty(player.Username);
            Assert.True(player.Id > 0);
            Assert.True(player.UpdatedAt < DateTimeOffset.Now);
            Assert.True(player.RegisteredAt < DateTimeOffset.Now);
        }

        [Fact]
        public async Task RecordLeaderBoardHasValidRecord() {
            int id = TestConfiguration.ValidGroupId;
            var metric = MetricType.Slayer;
            var period = Period.Month;

            var response = await _groupApi.RecordLeaderboards(id, metric, period);
            Record record = response.Data.Members.FirstOrDefault();

            Assert.True(record.Value > 0);
        }

        [Fact]
        public async Task SearchingGroupsWithoutNameResultsInMultipleResults() {
            var response = await _groupApi.Search();

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        [Fact]
        public async Task SearchingGroupsWithSpecificNameResultsInOneResult() {
            string specificGroupName = TestConfiguration.SpecificGroupName;

            var response = await _groupApi.Search(specificGroupName);

            Assert.NotNull(response);
            Assert.True(response.Data.Count() == 1);
            Assert.Equal(specificGroupName, response.Data.FirstOrDefault().Name, StringComparer.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task SearchingGroupsWithUnspecificNameAndPagingResultsInMultipleResultsWithPageLimit() {
            string unspecificGroupName = TestConfiguration.UnspecificGroupName;
            int pageLimit = 3;

            var response = await _groupApi.Search(unspecificGroupName, pageLimit, 0);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
            Assert.True(response.Data.Count() == pageLimit);
            Assert.Contains(unspecificGroupName, response.Data.FirstOrDefault().Name,
                StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task SearchingGroupsWithUnspecificNameResultsInMultipleResults() {
            string unspecificGroupName = TestConfiguration.UnspecificGroupName;

            var response = await _groupApi.Search(unspecificGroupName);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
            Assert.Contains(unspecificGroupName, response.Data.FirstOrDefault().Name,
                StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task StatisticsGivesValidStatistics() {
            int id = TestConfiguration.ValidGroupId;

            var response = await _groupApi.Statistics(id);

            Assert.NotEqual(0, response.Data.MaxedTotalPlayers);
            Assert.NotEqual(0, response.Data.Maxed200MExpPlayers);
            Assert.NotEqual(0, response.Data.MaxedCombatPlayers);
            Assert.NotNull(response.Data.AverageStats);
            Assert.NotNull(response.Data.AverageStats.AllMetrics);
        }

        [Fact]
        public async Task StatisticsSnapshotHoldsAllMetricsWithValues() {
            int id = TestConfiguration.ValidGroupId;

            var response = await _groupApi.Statistics(id);


            var types = EnumHelper.GetMetricTypes(MetricTypeCategory.All);
            var typesInResponse = new List<MetricType>();


            foreach (KeyValuePair<MetricType, Metric> metric in response.Data.AverageStats.AllMetrics) {
                Assert.NotNull(metric.Value);
                typesInResponse.Add(metric.Key);
            }

            foreach (var type in types) {
                Assert.Contains(type, typesInResponse);
            }
        }

        [Fact]
        public async Task ViewGroupResultsInValidGroup() {
            int id = TestConfiguration.ValidGroupId;

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
            int id = TestConfiguration.InvalidGroupId;

            Task Act() => _groupApi.View(id);
            var exception = await Assert.ThrowsAsync<BadRequestException>(Act);
            Assert.NotEmpty(exception.Message);
        }

        [Fact]
        public async Task ViewGroupWithValidIdResultsInGroupResult() {
            int id = TestConfiguration.ValidGroupId;

            var response = await _groupApi.View(id);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(id, response.Data.Id);
        }
    }
}