using System;
using System.Text.RegularExpressions;
using Discord;
using DiscordBotFanatic.Models.Configuration;

namespace DiscordBotFanatic.Helpers {
    public static class FanaticHelper {
        private static readonly Regex Regex = new Regex(@"\{\{([^\}]+)\}\}", RegexOptions.Compiled);

        public static string GetRandomDescription(MessageConfiguration messages) {
            Random rand = new Random();
            int i = rand.Next(messages.WaitMessages.Count);
            string result = messages.WaitMessages[i];

            string output = Regex.Replace(result, delegate(Match match) {
                return new Emoji(match.Groups[1].Value).ToString();
            });

            return output;
        }
    }
}