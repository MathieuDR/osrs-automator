using System.IO;
using System.Linq;
using System.Text.Json;
using Dashboard.Models.ApiRequests.DiscordEmbed;
using FluentAssertions;
using WebAppTests.Resources.EmbedJsons;
using Xunit;

namespace WebAppTests.Deserialization; 

public class DiscordEmbedDeserializationTests {
    [Theory]
    [MemberData(nameof(DiscordEmbedFiles.AllFiles), MemberType = typeof(DiscordEmbedFiles))]
    public void CanDeserializeIntoDiscordEmbed(string pathToJson) {
        var json = File.ReadAllText(pathToJson);

        var discordEmbeds = JsonSerializer.Deserialize<EmbedCollection>(json);

        discordEmbeds.Should().NotBeNull();
        discordEmbeds.Embeds.Should().NotBeNull();
        discordEmbeds.Embeds.Should().HaveCount(1);
    }

    [Fact]
    public void DeserializeIntoDiscordEmbedHasAuthor() {
        var json = File.ReadAllText(DiscordEmbedFiles.FirstFile);

        var discordEmbeds = JsonSerializer.Deserialize<EmbedCollection>(json);
        var discordEmbed = discordEmbeds.Embeds.FirstOrDefault();

        discordEmbed.Author.Should().NotBeNull();
        discordEmbed.Author.Name.Should().Be("ErkendRserke");
        discordEmbed.Author.Icon.Should().BeNullOrEmpty();
    }

    [Fact]
    public void DeserializeJsonWithImIntoDiscordEmbedHasImAuthorIcon() {
        var json = File.ReadAllText(DiscordEmbedFiles.FirstImFile);

        var discordEmbeds = JsonSerializer.Deserialize<EmbedCollection>(json);
        var discordEmbed = discordEmbeds.Embeds.FirstOrDefault();

        discordEmbed.Author.Icon.Should().Be("https://oldschool.runescape.wiki/images/0/09/Ironman_chat_badge.png");
    }
}