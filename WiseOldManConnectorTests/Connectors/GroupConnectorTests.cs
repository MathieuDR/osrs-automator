using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Interfaces;
using WiseOldManConnectorTests.Fixtures;

namespace WiseOldManConnectorTests.Connectors {
    public class GroupConnectorTests : ConnectorTests{
        public GroupConnectorTests(APIFixture fixture) : base(fixture) {
            _groupApi = fixture.ServiceProvider.GetService<IWiseOldManGroupApi>();
        }

        private readonly IWiseOldManGroupApi _groupApi;
    }
}