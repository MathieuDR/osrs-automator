using System.Text.RegularExpressions;
using Dashboard.Models.ApiRequests.DiscordEmbed;
using DiscordBot.Common.Dtos.Runescape;
using FluentResults;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace Dashboard.Transformers;

public class EmbedToRunescapeDropMapper : IMapper<Embed, RunescapeDrop> {
    public Result<RunescapeDrop> Map(Embed source) {
        var result = new RunescapeDrop();
        return Map(source, result);
    }

    public Result<RunescapeDrop> Map(Embed source, RunescapeDrop destination) {
        if (source == null) {
            return Result.Fail("Source is empty");
        }

        destination ??= new RunescapeDrop();

        if (source.Fields is null || source.Fields.Count == 0) {
            return Result.Fail("No fields present");
        }

        var recipientResult = SetRecipient(source, destination);
        if (recipientResult.IsFailed) {
            return recipientResult;
        }

        if (source.Description.ToLowerInvariant() == "just got a pet.") {
            destination = destination with { IsPet = true };
            return Result.Ok(destination);
        }

        var rarityResult = SetRarity(source, recipientResult.Value);
        if (rarityResult.IsFailed) {
            return rarityResult;
        }

        var patterns = new {
            Quantity = @"Just got (\d+)x",
            Brackets = @"\[([^\]^\n]+)\]",
            Urls = @"\(([^\)^\n]+)\)",
            Lvl = @"from lvl (\d+)"
        };

        var brackets = Regex.Matches(source.Description, patterns.Brackets);
        if (brackets.Count != 2) {
            return Result.Fail($"Unexpected matches for brackets: {brackets.Count}");
        }

        var urls = Regex.Matches(source.Description, patterns.Urls);
        if (urls.Count != 2) {
            return Result.Fail($"Unexpected matches for urls: {urls.Count}");
        }

        var lvl = Regex.Match(source.Description, patterns.Lvl);

        var item = destination.Item with { Name = brackets[0].Groups[1].Value, Url = urls[0].Groups[1].Value, Thumbnail = source.Thumbnail?.Url };
        var dropSource = destination.Source with {
            Name = brackets[1].Groups[1].Value, Url = urls[1].Groups[1].Value, Level = lvl.Success ? int.Parse(lvl.Groups[1].Value) : null
        };

        destination = rarityResult.Value with { Item = item, Source = dropSource };

        var quantity = Regex.Match(source.Description, patterns.Quantity);
        destination.Amount = quantity.Success ? int.Parse(quantity.Groups[1].Value) : 1;

        // Needs to be done after amount.
        var valueResult = SetValues(source, destination);
        return valueResult;
    }

    private static Result<RunescapeDrop> SetRecipient(Embed source, RunescapeDrop destination) {
        if (source.Author is null) {
            return Result.Fail("Author is empty");
        }


        var type = source.Author.Icon switch {
            string a when a.ToLower().Contains("hardcore") => PlayerType.HardcoreIronMan,
            string a when a.ToLower().Contains("ultimate") => PlayerType.UltimateIronMan,
            string a when a.ToLower().Contains("group") => PlayerType.IronMan,
            string a when a.ToLower().Contains("ironman") => PlayerType.IronMan,
            null => PlayerType.Regular,
            _ => PlayerType.Unknown
        };
        var recipient = destination.Recipient with { Username = source.Author.Name, IconUrl = source.Author.Icon, PlayerType = type };

        return Result.Ok(destination with { Recipient = recipient });
    }

    private static Result<RunescapeDrop> SetRarity(Embed source, RunescapeDrop destination) {
        var rarityField = source.Fields.FirstOrDefault(x => x.Name.ToLowerInvariant() == "rarity");
        if (rarityField is null) {
            return Result.Fail("No rarity field present");
        }

        var rarityString = rarityField.Value.Split(" ")[1].Substring(2);
        if (!float.TryParse(rarityString, out var rarity)) {
            return Result.Fail("Rarity is not a float");
        }

        return Result.Ok(destination with { Rarity = rarity });
    }

    private static Result<RunescapeDrop> SetValues(Embed source, RunescapeDrop destination) {
        var valueField = source.Fields.FirstOrDefault(x => x.Name.ToLowerInvariant() == "ge value");
        if (valueField is null) {
            return Result.Fail("No value field present");
        }

        var haValueField = source.Fields.FirstOrDefault(x => x.Name.ToLowerInvariant() == "ha value");
        if (haValueField is null) {
            return Result.Fail("No ha value field present");
        }

        var value = int.Parse(Regex.Match(valueField.Value, @"(\d+)").Captures[0].Value);
        var haValue = int.Parse(Regex.Match(haValueField.Value, @"(\d+)").Captures[0].Value);

        var item = destination.Item with { Value = value / destination.Amount, HaValue = haValue / destination.Amount };
        return Result.Ok(destination with { Item = item });
    }
}
