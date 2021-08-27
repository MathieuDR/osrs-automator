using System.Linq;
using System.Text.RegularExpressions;
using Dashboard.Models.ApiRequests.DiscordEmbed;
using DiscordBot.Common.Dtos.Runescape;
using FluentResults;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace Dashboard.Transformers {
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

            if (source.Fields is null) {
                return Result.Fail("No fields present");
            }

            var recipientResult = SetRecipient(source, destination);
            if (recipientResult.IsFailed) {
                return recipientResult;
            }

            if (source.Description.ToLowerInvariant() == "just got a pet.") {
                destination.IsPet = true;
                return Result.Ok(destination);
            }

            var rarityResult = SetRarity(source, destination);

            if (rarityResult.IsFailed) {
                return rarityResult;
            }

            if (source.Thumbnail is not null) {
                destination.Item.Thumbnail = source.Thumbnail.Url;
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

            destination.Item.Name = brackets[0].Groups[1].Value;
            destination.Source.Name = brackets[1].Groups[1].Value;

            var urls = Regex.Matches(source.Description, patterns.Urls);
            if (urls.Count != 2) {
                return Result.Fail($"Unexpected matches for urls: {urls.Count}");
            }

            destination.Item.Url = urls[0].Groups[1].Value;
            destination.Source.Url = urls[1].Groups[1].Value;

            var quantity = Regex.Match(source.Description, patterns.Quantity);
            destination.Amount = quantity.Success ? int.Parse(quantity.Groups[1].Value) : 1;

            var lvl = Regex.Match(source.Description, patterns.Lvl);
            destination.Source.Level = lvl.Success ? int.Parse(lvl.Groups[1].Value) : null;

            // Needs to be done after amount.
            var valueResult = SetValues(source, destination);
            if (valueResult.IsFailed) {
                return valueResult;
            }

            return Result.Ok(destination);
        }

        private static Result SetRecipient(Embed source, RunescapeDrop destination) {
            if (source.Author is null) {
                return Result.Fail("Author is empty");
            }

            destination.Recipient.Username = source.Author.Name;
            destination.Recipient.IconUrl = source.Author.Icon;
            destination.Recipient.PlayerType = source.Author.Icon switch {
                string a when a.ToLower().Contains("hardcore") => PlayerType.HardcoreIronMan,
                string a when a.ToLower().Contains("ultimate") => PlayerType.UltimateIronMan,
                string a when a.ToLower().Contains("ironman") => PlayerType.IronMan,
                null => PlayerType.Regular,
                _ => PlayerType.Unknown
            };

            return Result.Ok();
        }

        private static Result SetRarity(Embed source, RunescapeDrop destination) {
            var rarityField = source.Fields.FirstOrDefault(x => x.Name.ToLowerInvariant() == "rarity");
            if (rarityField is null) {
                return Result.Fail("No rarity field present");
            }

            var rarityString = rarityField.Value.Split(" ")[1].Substring(2);
            if (!float.TryParse(rarityString, out var rarity)) {
                return Result.Fail("Rarity is not a float");
            }

            destination.Rarity = rarity;
            return Result.Ok();
        }

        private static Result SetValues(Embed source, RunescapeDrop destination) {
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

            destination.Item.Value = value / destination.Amount;
            destination.Item.HaValue = haValue / destination.Amount;

            return Result.Ok();
        }
    }
}
