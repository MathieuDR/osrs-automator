using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models.Output.Exceptions;
using WiseOldManConnectorTests.Fixtures;
using Xunit;

namespace WiseOldManConnectorTests.Connectors {
    public class BaseConnectorTests : ConnectorTests {
        public BaseConnectorTests(ApiFixture fixture) : base(fixture) {
            _playerApi = fixture.ServiceProvider.GetService<IWiseOldManPlayerApi>();
        }

        private readonly IWiseOldManPlayerApi _playerApi;

        [Fact]
        public async Task ViewPlayerByInvalidId() {
            int id = -1;

            Task Act() => _playerApi.View(id);

            var exception = await Assert.ThrowsAsync<BadRequestException>(Act);

            Assert.NotNull(exception);
            Assert.NotEmpty(exception.Parameters);
            Assert.False(string.IsNullOrWhiteSpace(exception.Message));
            Assert.Equal(exception.Message, exception.WiseOldManMessage);
        }
    }
}