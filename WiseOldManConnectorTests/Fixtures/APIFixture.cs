using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Configuration;

namespace WiseOldManConnectorTests.Fixtures {
    public class ApiFixture {
        public ApiFixture() {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddWiseOldManApi();
            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; set; }
    }
}
