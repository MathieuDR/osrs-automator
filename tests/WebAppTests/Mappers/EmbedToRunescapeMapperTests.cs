using System.Text.Json;
using DiscordBot.Common.Dtos.Runescape;
using DiscordBot.Dashboard.Models.ApiRequests.DiscordEmbed;
using DiscordBot.Dashboard.Transformers;
using FluentAssertions;
using WebAppTests.Resources.EmbedJsons;
using WiseOldManConnector.Models.WiseOldMan.Enums;
using Xunit;

namespace WebAppTests.Mappers;

public class EmbedToRunescapeMapperTests {
    private readonly IMapper<Embed, RunescapeDrop> _mapper;

    public EmbedToRunescapeMapperTests() {
        _mapper = new EmbedToRunescapeDropMapper();
    }

    private Embed GetEmbedFromFile(string path) {
        var json = File.ReadAllText(path);
        var discordEmbeds = JsonSerializer.Deserialize<EmbedCollection>(json);
        return discordEmbeds.Embeds.FirstOrDefault();
    }

    [Theory]
    [MemberData(nameof(DiscordEmbedFiles.AllFiles), MemberType = typeof(DiscordEmbedFiles))]
    public void MappingShouldNotBeNull(string pathToJson) {
        var result = _mapper.Map(GetEmbedFromFile(pathToJson));
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Theory]
    [MemberData(nameof(DiscordEmbedFiles.WithPlayerTypes), MemberType = typeof(DiscordEmbedFiles))]
    public void MappingShouldHaveCorrectPlayerType(string path, PlayerType type) {
        var result = _mapper.Map(GetEmbedFromFile(path));
        var value = result.Value;

        value.Recipient.PlayerType.Should().Be(type);
    }

    [Theory]
    [MemberData(nameof(DiscordEmbedFiles.WithQuantities), MemberType = typeof(DiscordEmbedFiles))]
    public void MappingShouldHaveCorrectQuantity(string path, int quantity) {
        var result = _mapper.Map(GetEmbedFromFile(path));
        var value = result.Value;

        value.Amount.Should().Be(quantity);
    }

    [Theory]
    [MemberData(nameof(DiscordEmbedFiles.WithNames), MemberType = typeof(DiscordEmbedFiles))]
    public void MappingShouldHaveCorrectName(string path, string name) {
        var result = _mapper.Map(GetEmbedFromFile(path));
        var value = result.Value;

        value.Recipient.Username.Should().Be(name);
    }

    [Theory]
    [MemberData(nameof(DiscordEmbedFiles.WithRarity), MemberType = typeof(DiscordEmbedFiles))]
    public void MappingShouldHaveCorrectRarities(string path, float rarity) {
        var result = _mapper.Map(GetEmbedFromFile(path));
        var drop = result.Value;

        drop.Rarity.Should().Be(rarity);
        drop.RarityPercent.Should().Be(1 / rarity);
    }

    [Theory]
    [MemberData(nameof(DiscordEmbedFiles.WithItem), MemberType = typeof(DiscordEmbedFiles))]
    public void MappingShouldHaveCorrectItemInfo(string path, string item, string url, string icon) {
        var result = _mapper.Map(GetEmbedFromFile(path));
        var drop = result.Value;

        drop.Item.Name.Should().Be(item);
        drop.Item.Url.Should().Be(url);
        drop.Item.Thumbnail.Should().Be(icon);
    }

    [Theory]
    [MemberData(nameof(DiscordEmbedFiles.WithSources), MemberType = typeof(DiscordEmbedFiles))]
    public void MappingShouldHaveCorrectSourceInfo(string path, string name, int? level, string url) {
        var result = _mapper.Map(GetEmbedFromFile(path));
        var drop = result.Value;

        drop.Source.Name.Should().Be(name);
        drop.Source.Url.Should().Be(url);
        drop.Source.Level.Should().Be(level);
    }

    [Theory]
    [MemberData(nameof(DiscordEmbedFiles.WithValues), MemberType = typeof(DiscordEmbedFiles))]
    public void MappingShouldHaveCorrectValues(string path, int amount, int value, int haValue) {
        var result = _mapper.Map(GetEmbedFromFile(path));
        var drop = result.Value;

        drop.TotalValue.Should().Be(value);
        drop.TotalHaValue.Should().Be(haValue);
        drop.Item.Value.Should().Be(value / amount);
        drop.Item.HaValue.Should().Be(haValue / amount);
    }
}
