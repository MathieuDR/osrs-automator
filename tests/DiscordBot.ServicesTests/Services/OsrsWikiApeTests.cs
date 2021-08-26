using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Services.Configuration;
using DiscordBot.Services.ExternalServices;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DiscordBot.ServicesTests.Services {
    public class OsrsWikiApeTests {
        [Fact]
        public async Task OsrsWikiShouldReturnPageWithContent() {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDiscordBotServices();
            var provider = serviceCollection.BuildServiceProvider();

            var api = provider.GetRequiredService<IOsrsWikiApi>();
            var request = await api.GetPage("Collection log");

            request.Should().NotBeNull();
            request.Query.Should().NotBeNull();
            request.Query.Pages.Should().HaveCount(1);
            request.Query.Pages.FirstOrDefault().Value.Should().NotBeNull();
            request.Query.Pages.FirstOrDefault().Value.Revisions.Should().HaveCount(1);
            request.Query.Pages.FirstOrDefault().Value.Revisions.FirstOrDefault().Content.Should().NotBeNullOrEmpty();
        }
        
        [Fact]
        public async Task OsrsWikiShouldReturnCorrectPage() {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDiscordBotServices();
            var provider = serviceCollection.BuildServiceProvider();

            var api = provider.GetRequiredService<IOsrsWikiApi>();
            var request = await api.GetPage("Collection log");

            request.Should().NotBeNull();
            request.Query.Should().NotBeNull();
            request.Query.Pages.Should().ContainKey("199935");
        }
        
        [Fact]
        public void OsrsWikiShouldBeProvided() {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDiscordBotServices();
            var provider = serviceCollection.BuildServiceProvider();

            var api = provider.GetRequiredService<IOsrsWikiApi>();

            api.Should().NotBeNull();
        }
    }
}
