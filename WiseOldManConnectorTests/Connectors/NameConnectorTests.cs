using System.Threading.Tasks;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models.Output.Exceptions;
using WiseOldManConnectorTests.Configuration;
using WiseOldManConnectorTests.Fixtures;
using Xunit;

namespace WiseOldManConnectorTests.Connectors {
    public class NameConnectorTests : BaseConnectorTests {
        private readonly IWiseOldManNameApi _nameApi;

        public NameConnectorTests(ApiFixture fixture) : base(fixture) {
            _nameApi = fixture.ServiceProvider.GetRequiredService<IWiseOldManNameApi>();
        }

        private string GetName(int chars) {
            var toReplace = "";
            for (var i = 0; i < chars; i++) {
                toReplace += "*";
            }

            var faker = new Faker();
            return faker.Random.Replace(toReplace).ToLowerInvariant();
        }

        [Fact]
        public async Task CanRequestNameChange() {
            var username = TestConfiguration.ValidAccomplishedPlayer.ToLowerInvariant();
            var newName = GetName(10);

            var response = await _nameApi.Request(username, newName);

            Assert.NotNull(response);
            Assert.NotNull(response.Data);
            Assert.Equal(username.ToLower(), response.Data.OldName.ToLower());
            Assert.Equal(newName.ToLower(), response.Data.NewName.ToLower());
        }

        [Fact]
        public async Task NameChangeWithInvalidNameGivesBadRequest() {
            var username = TestConfiguration.SecondaryValidPlayerUserName.ToLowerInvariant();
            var newName = GetName(30);


            Task Act() {
                return _nameApi.Request(username, newName);
            }

            var exception = await Assert.ThrowsAnyAsync<BadRequestException>(Act);

            Assert.NotNull(exception);
            Assert.NotEmpty(exception.Message);
            Assert.Contains("invalid new name.", exception.Message.ToLowerInvariant());
        }


        // [Fact]
        // public async Task DoubleNameChangeWithValidNameGivesBadRequest() {
        //     var username = TestConfiguration.ValidHardcoreIronMan.ToLowerInvariant();
        //     var newName = GetName(10);
        //
        //
        //     Task Act() {
        //         return _nameApi.Request(username, newName);
        //     }
        //
        //     await Act();
        //     var exception = await Assert.ThrowsAnyAsync<BadRequestException>(Act);
        //
        //     Assert.NotNull(exception);
        //     Assert.NotEmpty(exception.Message);
        //     Assert.Contains("There's already a similar pending name change.".ToLowerInvariant(), exception.Message.ToLowerInvariant());
        // }
    }
}
