using System;
using System.Text.RegularExpressions;
using Discord;
using DiscordBot.Common.Configuration;

namespace DiscordBot.Helpers {
    public static class MessageHelper {
        private static readonly Regex EmojiRegex = new(@"\{\{([^\}]+)\}\}", RegexOptions.Compiled);

        public static string GetRandomDescription(MessageConfiguration messages) {
            var rand = new Random();
            var i = rand.Next(messages.WaitMessages.Count);
            var result = messages.WaitMessages[i];

            var output = EmojiRegex.Replace(result,
                delegate(Match match) { return new Emoji(match.Groups[1].Value).ToString(); });

            return output;
        }
    }
}
