using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models.Output.Exceptions;
using WiseOldManConnectorTests.Fixtures;
using Xunit;

namespace WiseOldManConnectorTests.Connectors;

public class BaseConnectorTests : ConnectorTests {
    private readonly IWiseOldManPlayerApi _playerApi;

    public BaseConnectorTests(ApiFixture fixture) : base(fixture) {
        _playerApi = fixture.ServiceProvider.GetService<IWiseOldManPlayerApi>();
    }

    [Fact]
    public async Task ViewPlayerByInvalidId() {
        var id = -1;

        Task Act() {
            return _playerApi.View(id);
        }

        var exception = await Assert.ThrowsAsync<BadRequestException>(Act);

        Assert.NotNull(exception);
        Assert.NotEmpty(exception.Parameters);
        Assert.False(string.IsNullOrWhiteSpace(exception.Message));
        Assert.Equal(exception.Message, exception.WiseOldManMessage);
    }
}
