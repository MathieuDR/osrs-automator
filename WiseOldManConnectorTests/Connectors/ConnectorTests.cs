using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnectorTests.Fixtures;
using Xunit;

namespace WiseOldManConnectorTests.Connectors {
    public class ConnectorTests : IClassFixture<ApiFixture> {
        public ServiceProvider ServiceProvider;

        public ConnectorTests(ApiFixture fixture) {
            ServiceProvider = fixture.ServiceProvider;
        }
    }
}
