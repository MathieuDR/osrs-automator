using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.ExternalServices;
using DiscordBot.Services.Models.MediaWikiApi;
using DiscordBot.Services.Services;
using DiscordBot.ServicesTests.Resources.EmbedJsons;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace DiscordBot.ServicesTests.Services {
    public class CollectionLogItemProviderTests {
        private const string WikiPage = "Collection log";

        private WikiResponseFiles.File GetFile() {
            return WikiResponseFiles.Files.FirstOrDefault();
        }

        private IOsrsWikiApi GetWikiMock() {
            var json = File.ReadAllText(GetFile().Path);
            var response = JsonSerializer.Deserialize<QueryResponse>(json);


            var mock = Substitute.For<IOsrsWikiApi>();
            mock.GetPage(Arg.Any<string>()).Returns(response);

            return mock;
        }

        [Fact]
        public async Task RequestItemsDoesNotFail() {
            var sut = new CollectionLogItemProvider(Substitute.For<ILogger<CollectionLogItemProvider>>(), GetWikiMock());

            var result = await sut.GetCollectionLogItemNames();
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task RequestItemsProvidesListOfItems() {
            var sut = new CollectionLogItemProvider(Substitute.For<ILogger<CollectionLogItemProvider>>(), GetWikiMock());

            var result = await sut.GetCollectionLogItemNames();
            result.Value.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task RequestItemsRequestsApiOnce() {
            var mock = GetWikiMock();
            var sut = new CollectionLogItemProvider(Substitute.For<ILogger<CollectionLogItemProvider>>(), mock);

            _ = await sut.GetCollectionLogItemNames();
            await mock.Received(1).GetPage(WikiPage);
        }

        [Fact]
        public async Task RequestItemsRequestsApiAfterReset() {
            var mock = GetWikiMock();
            var sut = new CollectionLogItemProvider(Substitute.For<ILogger<CollectionLogItemProvider>>(), mock);

            _ = await sut.GetCollectionLogItemNames();
            sut.ResetCache();
            _ = await sut.GetCollectionLogItemNames();

            await mock.Received(2).GetPage(WikiPage);
        }

        [Fact]
        public async Task RequestItemsUsesParses() {
            var sut = new CollectionLogItemProvider(Substitute.For<ILogger<CollectionLogItemProvider>>(), GetWikiMock());

            var result = (await sut.GetCollectionLogItemNames()).Value;
            result.Should().Contain("Rum");
        }
    }
}
