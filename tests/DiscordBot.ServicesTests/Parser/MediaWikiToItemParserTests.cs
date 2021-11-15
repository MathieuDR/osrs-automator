using System.Text.Json;
using DiscordBot.Services.Models.MediaWikiApi;
using DiscordBot.Services.Parsers;
using DiscordBot.ServicesTests.Resources.EmbedJsons;
using FluentAssertions;
using Xunit;

namespace DiscordBot.ServicesTests.Parser;

public class MediaWikiToItemParserTests {
    private string GetContentString(WikiResponseFiles.File file = null) {
        file ??= WikiResponseFiles.Files.FirstOrDefault();
        var json = File.ReadAllText(file.Path);
        var response = JsonSerializer.Deserialize<QueryResponse>(json);

        return response.Query.Pages.FirstOrDefault().Value.Revisions.FirstOrDefault().Content;
    }

    [Fact]
    public void ParsingDoesNotFail() {
        var str = GetContentString();
        var result = MediaWikiContentToItemsParser.GetItems(str);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailed.Should().BeFalse();
        result.Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ParsingShouldFailForBadString() {
        var str = "badstring";
        var result = MediaWikiContentToItemsParser.GetItems(str);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void ParsingShouldHaveAllItems() {
        var file = WikiResponseFiles.Files.FirstOrDefault();
        var str = GetContentString(file);
        var result = MediaWikiContentToItemsParser.GetItems(str);

        result.Should().NotBeNull();
        result.Value.Should().HaveCount(file.ItemQuantities);
    }

    [Fact]
    public void ParsingShouldHaveItemNames() {
        var file = WikiResponseFiles.Files.FirstOrDefault();
        var str = GetContentString(file);
        var result = MediaWikiContentToItemsParser.GetItems(str);

        result.Should().NotBeNull();
        result.Value.Should().Contain("Abyssal orphan");
        result.Value.Should().Contain("Dragon thrownaxe");
    }


    [Fact]
    public void ParsingShouldHaveItemNamesWithApostrophes() {
        var file = WikiResponseFiles.Files.FirstOrDefault();
        var str = GetContentString(file);
        var result = MediaWikiContentToItemsParser.GetItems(str);

        result.Should().NotBeNull();
        result.Value.Should().Contain("Verac's brassard");
        result.Value.Should().Contain("Inquisitor's great helm");
    }

    [Fact]
    public void ParsingShouldHaveItemNamesWithDashes() {
        var file = WikiResponseFiles.Files.FirstOrDefault();
        var str = GetContentString(file);
        var result = MediaWikiContentToItemsParser.GetItems(str);

        result.Should().NotBeNull();
        result.Value.Should().Contain("Zul-andra teleport");
    }

    [Fact]
    public void ParsingShouldHaveItemNamesWithNumbers() {
        var file = WikiResponseFiles.Files.FirstOrDefault();
        var str = GetContentString(file);
        var result = MediaWikiContentToItemsParser.GetItems(str);

        result.Should().NotBeNull();
        result.Value.Should().Contain("Godsword shard 1");
        result.Value.Should().Contain("Sinhaza shroud tier 4");
    }

    [Fact]
    public void ParsingShouldHaveItemNamesWithBraces() {
        var file = WikiResponseFiles.Files.FirstOrDefault();
        var str = GetContentString(file);
        var result = MediaWikiContentToItemsParser.GetItems(str);

        result.Should().NotBeNull();
        result.Value.Should().Contain("Bronze platelegs (t)");
        result.Value.Should().Contain("Sinhaza shroud tier 4");
    }

    [Fact]
    public void ParsingShouldHaveItemNamesWithSpecialCases() {
        var file = WikiResponseFiles.Files.FirstOrDefault();
        var str = GetContentString(file);
        var result = MediaWikiContentToItemsParser.GetItems(str);

        result.Should().NotBeNull();
        result.Value.Should().Contain("Supply crate");
    }
}
