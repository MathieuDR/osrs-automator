using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models.WiseOldMan.Enums;
using WiseOldManConnectorTests.Fixtures;
using Xunit;

namespace WiseOldManConnectorTests.Connectors {
    public class CompetitionConnectorTests : ConnectorTests{
        public CompetitionConnectorTests(APIFixture fixture) : base(fixture) {
            _competitionApi = fixture.ServiceProvider.GetRequiredService<IWiseOldManCompetitionApi>();
        }

        private readonly IWiseOldManCompetitionApi _competitionApi;

        [Fact]
        public async Task ViewCompetitionHasCompetitionForValidId() {
            var competitionId = 785;

            var response = await _competitionApi.View(competitionId);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
        }

        [Fact]
        public async Task ViewCompetitionReturnsValidCompetition() {
            var competitionId = 785;

            var response = await _competitionApi.View(competitionId);
            var competition = response.Data;

            Assert.Equal(competitionId, competition.Id);
            Assert.True(competition.ParticipantCount > 0);
            Assert.Equal("The great volcanic mine xp grab", competition.Title);
            Assert.Equal(new DateTimeOffset(2020,11,16,12,0,0, TimeSpan.Zero), competition.StartDate);
            Assert.Equal(new DateTimeOffset(2020,11,22,20,0,0, TimeSpan.Zero), competition.EndDate);
            Assert.Equal("6 days, 8 hours", competition.Duration);
            Assert.Equal(MetricType.Mining, competition.Metric);
            Assert.Equal(51, competition.GroupId);
            Assert.True(competition.Score > 0);
            Assert.Equal(competition.ParticipantCount, competition.Participants.Count);
        }

        [Fact]
        public async Task ViewCompetitionThrowsExceptionForNegativeId() {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task ViewCompetitionForInvalidIdGivesNullResult() {
            throw new NotImplementedException();
        }

    }
}