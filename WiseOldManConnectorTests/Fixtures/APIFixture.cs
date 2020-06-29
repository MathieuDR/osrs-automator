using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Configuration;

namespace WiseOldManConnectorTests.Fixtures {
    public class APIFixture {
        public APIFixture() {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWiseOldManApi();
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; set; }
    }
}