using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FluentResults;
using Quartz.Util;

namespace DiscordBot.Services.Parsers {
    public class MediaWikiContentToItemsParser {
        private const string RegexPattern = @"{{plink\|(?<item>[^}|]+)(?>\|pic[^}|]+)?(?>\|txt=(?<itemTxt>[^}|]+))?[^}]*}}";

        public static Result<IEnumerable<string>> GetItems(string mediaWikiContent) {
            var matches = Regex.Matches(mediaWikiContent, RegexPattern);
            if (!matches.Any()) {
                return Result.Fail("No regex matches found");
            }

            var result = new List<string>();

            foreach (Match match in matches) {
                // var item = match.Value;//match.Groups["item"].Value;
                var item = match.Groups["item"].Value;
                var itemText = match.Groups["itemTxt"].Value;
                item = !itemText.IsNullOrWhiteSpace() ? itemText : item;

                if (!result.Contains(item)) {
                    result.Add(item);
                }
            }

            return Result.Ok((IEnumerable<string>) result);
        }
    }
}
