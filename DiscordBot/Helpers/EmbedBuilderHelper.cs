using System;
using System.Collections.Generic;
using Discord;
using Discord.Commands;
using DiscordBot.Common.Models.Decorators;

namespace DiscordBot.Helpers {
    public static class EmbedBuilderHelper {
        public static EmbedBuilder AddWiseOldMan(this EmbedBuilder builder, string title = "Wiseoldman.net",
            string url = "http://www.wiseoldman.net") {
            builder.AddCommonProperties();
            builder.AddWiseldManAuthor(title, url);
            return builder;
        }

        public static EmbedBuilder AddWiseOldMan<T>(this EmbedBuilder builder, ItemDecorator<T> decorator) {
            return builder.AddWiseOldMan(decorator.Title, decorator.Link);
        }


        public static EmbedBuilder AddWiseldManAuthor(this EmbedBuilder builder, string title = "Wiseoldman.net",
            string url = "http://www.wiseoldman.net") {
            builder.Author = new EmbedAuthorBuilder {
                Name = title,
                IconUrl = "https://wiseoldman.net/img/logo.png",
                Url = url
            };

            return builder;
        }

        public static EmbedBuilder AddWiseOldManAuthor<T>(this EmbedBuilder builder, ItemDecorator<T> decorator) {
            return builder.AddWiseldManAuthor(decorator.Title, decorator.Link);
        }

        public static EmbedBuilder AddWiseOldManAuthor(this EmbedBuilder builder, string title = null,
            string resourceUrl = null) {
            builder.Author = new EmbedAuthorBuilder {
                Name = string.IsNullOrWhiteSpace(title) ? "Wiseoldman.net" : title,
                IconUrl = "https://wiseoldman.net/img/logo.png",
                Url = string.IsNullOrWhiteSpace(resourceUrl) ? "" : resourceUrl
            };

            return builder;
        }

        public static EmbedBuilder AddCommonProperties(this EmbedBuilder builder) {
            builder.Timestamp = DateTimeOffset.Now;
            builder.Color = Color.DarkPurple;

            return builder;
        }

        public static EmbedBuilder WithMessageAuthorFooter(this EmbedBuilder builder, ICommandContext context,
            string appendToFooter = "") {
            var footerText = $"Requested by {context.User.Username}.";
            if (!string.IsNullOrWhiteSpace(appendToFooter)) {
                footerText += $", {appendToFooter}";
            }

            builder.Footer = new EmbedFooterBuilder {
                IconUrl = context.User.GetAvatarUrl(),
                Text = footerText
            };

            return builder;
        }


        public static EmbedBuilder CreateAuthorFromMessageAuthor(this EmbedBuilder builder, ICommandContext context,
            string url = null,
            string appendToUsername = null) {
            var footerText = $"{context.User.Username}";

            if (!string.IsNullOrWhiteSpace(appendToUsername)) {
                footerText += $", {appendToUsername}";
            }

            builder.Author = new EmbedAuthorBuilder {
                IconUrl = context.User.GetAvatarUrl(),
                Name = footerText,
                Url = url
            };

            return builder;
        }

        public static EmbedBuilder AddFieldsFromDictionary(this EmbedBuilder builder, Dictionary<string, string> dictionary) {
            foreach (var pair in dictionary) {
                builder.AddField(pair.Key, pair.Value);
            }

            return builder;
        }
    }
}
