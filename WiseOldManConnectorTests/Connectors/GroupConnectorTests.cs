using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.Output.Exceptions;
using WiseOldManConnectorTests.Configuration;
using WiseOldManConnectorTests.Fixtures;
using Xunit;

namespace WiseOldManConnectorTests.Connectors {
    public class GroupConnectorTests : ConnectorTests{
        public GroupConnectorTests(APIFixture fixture) : base(fixture) {
            _groupApi = fixture.ServiceProvider.GetService<IWiseOldManGroupApi>();
        }

        private readonly IWiseOldManGroupApi _groupApi;

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
        public async Task SearchingGroupsWithUnspecificNameResultsInMultipleResults() {
            string unspecificGroupName = TestConfiguration.UnspecificGroupName;

            var response = await _groupApi.Search(unspecificGroupName);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
            Assert.Contains(unspecificGroupName, response.Data.FirstOrDefault().Name, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task SearchingGroupsWithUnspecificNameAndPagingResultsInMultipleResultsWithPageLimit() {
            string unspecificGroupName = TestConfiguration.UnspecificGroupName;
            int pageLimit = 3;

            var response = await _groupApi.Search(unspecificGroupName, pageLimit, 0);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
            Assert.True(response.Data.Count() == pageLimit);
            Assert.Contains(unspecificGroupName, response.Data.FirstOrDefault().Name, StringComparison.InvariantCultureIgnoreCase);
        }

        [Fact]
        public async Task ViewGroupWithValidIdResultsInGroupResult() {
            int id = TestConfiguration.ValidGroupId;

            var response = await _groupApi.View(id);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(id, response.Data.Id);
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
        public async Task MembersFromValidGroupResultsInMultipleMembers() {
            int id = TestConfiguration.ValidGroupId;

            var response = await _groupApi.GetMembers(id);

            Assert.NotNull(response);
            Assert.NotEmpty(response.Data);
        }

        
        [Fact]
        public async Task MembersFromGroupResultInValidPlayers() {
            int id = TestConfiguration.ValidGroupId;

            var response = await _groupApi.GetMembers(id);
            var player = response.Data.FirstOrDefault();
            
            Assert.NotEmpty(player.DisplayName);
            Assert.NotEmpty(player.Username);
            Assert.True(player.Id > 0);
            //Assert.True(player.CombatLevel > 3);
            Assert.True(player.OverallExperience > 1000);
            //Assert.NotNull(player.LatestSnapshot);
            Assert.NotNull(player.Role);
            Assert.True(player.UpdatedAt < DateTimeOffset.Now);
            Assert.True(player.RegisteredAt < DateTimeOffset.Now);
        }
    }
}