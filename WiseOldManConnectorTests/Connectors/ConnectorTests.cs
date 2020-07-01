using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Interfaces;
using WiseOldManConnectorTests.Fixtures;
using Xunit;

namespace WiseOldManConnectorTests.Connectors {
    public class ConnectorTests : IClassFixture<APIFixture> {
        
        public ServiceProvider ServiceProvider;

        public ConnectorTests(APIFixture fixture) {
            ServiceProvider = fixture.ServiceProvider;
        }
    }
}