using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBotFanatic.Models.ResponseModels;
using Microsoft.Extensions.Configuration;

namespace DiscordBotFanatic.Helpers {
    public static class DiscordHelper {
        public static void AddEmptyField(this EmbedBuilder builder, bool inline = false) {
            builder.AddField("\u200B", "\u200B", inline);
        }

        public static async Task AddPagingEmojis(this IMessage message, bool trash = false) {
            var leftEmoji = Emote.Parse("\u2190");
            var rightEmoji = Emote.Parse("\u2192");
            var trashEmoji = Emote.Parse("\U0001F5D1");
            await message.AddReactionAsync(leftEmoji, RequestOptions.Default);
            await message.AddReactionAsync(trashEmoji, RequestOptions.Default);
            await message.AddReactionAsync(rightEmoji, RequestOptions.Default);
        }

        public static CustomPaginatedMessage AddPagingToFooter(this CustomPaginatedMessage message) {
            message.EmbedWrapper.Footer ??= new EmbedFooterBuilder();
            string whitespace = " ";
            if (string.IsNullOrWhiteSpace(message.EmbedWrapper.Footer.Text)) {
                whitespace = "";
            }
            message.EmbedWrapper.Footer.Text += $"{whitespace}{{0}}/{{1}} Pages.";
            return message;
        }
    }
}