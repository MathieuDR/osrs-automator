using System;
using System.Text.RegularExpressions;
using Discord;
using DiscordBotFanatic.Models.Configuration;

namespace DiscordBotFanatic.Helpers {
    public static class FanaticHelper {
        static readonly Regex re = new Regex(@"\{\{([^\}]+)\}\}", RegexOptions.Compiled);

        public static string GetRandomDescription(MessageConfiguration messages) {
            Random rand = new Random();
            int i = rand.Next(messages.WaitMessages.Count);
            string result = messages.WaitMessages[i];

            string output = re.Replace(result, delegate(Match match) {
                return new Emoji(match.Groups[1].Value).ToString();
            });

            return output;
        }
    }
}