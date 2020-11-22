﻿using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.Output.Exceptions;
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
            Assert.Equal(new DateTimeOffset(2020,11,16,12,0,0, new TimeSpan(1,0,0)), competition.StartDate);
            Assert.Equal(new DateTimeOffset(2020,11,22,20,0,0, new TimeSpan(1,0,0)), competition.EndDate);
            Assert.Equal("6 days, 8 hours", competition.Duration);
            Assert.Equal(MetricType.Mining, competition.Metric);
            Assert.Equal(51, competition.GroupId);
            Assert.True(competition.Score > 0);
            Assert.Equal(competition.ParticipantCount, competition.Participants.Count);
        }

        [Fact]
        public async Task ViewCompetitionReturnsValidCompetitionParticepants() {
            var competitionId = 785;

            var response = await _competitionApi.View(competitionId);
            var participants = response.Data.Participants;

            Assert.NotNull(participants);
            Assert.NotEmpty(participants);
            Assert.Equal(response.Data.ParticipantCount, participants.Count);
            for (var i = 0; i < participants.Count; i++) {
                CompetitionParticipant competitionParticipant = participants[i];

                Assert.NotNull(competitionParticipant.Player);
                Assert.NotNull(competitionParticipant.CompetitionDelta);
                Assert.NotNull(competitionParticipant.History);

                
                // Delta
                if (competitionParticipant.CompetitionDelta.End > 0) {
                    Assert.True(competitionParticipant.CompetitionDelta.End >= competitionParticipant.CompetitionDelta.Start);
                    Assert.Equal(competitionParticipant.CompetitionDelta.Gained,
                        competitionParticipant.CompetitionDelta.End - competitionParticipant.CompetitionDelta.Start);
                }

                //History
                if (competitionParticipant.CompetitionDelta.Gained > 0 && i < 5) {
                    Assert.NotEmpty(competitionParticipant.History);
                } else {
                    Assert.Empty(competitionParticipant.History);
                }

                //Player
                var player = competitionParticipant.Player;
                Assert.NotEmpty(player.DisplayName);
                Assert.NotEmpty(player.Username);
                Assert.NotEmpty(player.Username);
                Assert.NotEqual(PlayerType.Unknown, player.Type);
                Assert.True(player.Id > 0);
            }
        }

        [Fact]
        public async Task ViewCompetitionThrowsBadRequestExceptionForNegativeId() {
            var competitionId = int.MaxValue;


            Task Act() => _competitionApi.View(competitionId);
            await Assert.ThrowsAsync<BadRequestException>(Act);
        }

        [Fact]
        public async Task ViewCompetitionForInvalidIdGivesBadRequestException() {
            var competitionId = int.MaxValue;


            Task Act() => _competitionApi.View(competitionId);
            await Assert.ThrowsAsync<BadRequestException>(Act);
        }

        [Fact]
        public async Task ViewCompetitionForInvalidIdGivesValidNotFoundExceptionMessage() {
            var competitionId = int.MaxValue;


            Task Act() => _competitionApi.View(competitionId);
            var exception = await Assert.ThrowsAsync<BadRequestException>(Act);

            Assert.NotEmpty(exception.Message);
            Assert.Equal("Competition not found.",exception.Message);
        }
        [Fact]
        public async Task ViewCompetitionForInvalidIdGivesValidNotFoundStatusCode() {
            var competitionId = int.MaxValue;


            Task Act() => _competitionApi.View(competitionId);
            var exception = await Assert.ThrowsAsync<BadRequestException>(Act);

            Assert.Equal(HttpStatusCode.NotFound,exception.StatusCode);
        }
    }
}