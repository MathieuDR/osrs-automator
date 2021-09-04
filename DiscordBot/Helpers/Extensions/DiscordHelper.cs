using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Models.Contexts;

namespace DiscordBot.Helpers.Extensions {
    public static class DiscordHelper {
        public static void AddEmptyField(this EmbedBuilder builder, bool inline = false) {
            builder.AddField("\u200B", "\u200B", inline);
        }


        public static IEnumerable<string> ParseMentions(this IEnumerable<string> strings, SocketCommandContext context) {
            foreach (var @string in strings) {
                yield return @string.ParseMentions(context);
            }
        }

        /// <summary>
        ///     Parse a string that might contain a discord mention
        /// </summary>
        /// <param name="input">A string</param>
        /// <param name="context">Context of the command, For guilds</param>
        /// <returns></returns>
        public static string ParseMentions(this string input, SocketCommandContext context) {
            var args = input.ToCollectionOfParameters();
            var parsedStrings = new List<string>();

            foreach (var arg in args) {
                if (arg.StartsWith('<') && arg.Contains('>')) {
                    var indexOfEnd = arg.IndexOf('>');
                    var substr = arg;
                    var toAdd = "";
                    if (indexOfEnd != arg.Length - 1) {
                        substr = arg.Substring(0, indexOfEnd + 1);
                        toAdd = arg.Substring(indexOfEnd + 1);
                    }

                    if (MentionUtils.TryParseUser(substr, out var userId)) {
                        parsedStrings.Add(context.Guild.GetUser(userId).DisplayName() + toAdd);
                        continue;
                    }

                    if (MentionUtils.TryParseRole(substr, out var roleId)) {
                        parsedStrings.Add(context.Guild.GetRole(roleId).Name + toAdd);
                        continue;
                    }

                    if (MentionUtils.TryParseChannel(substr, out var channelId)) {
                        parsedStrings.Add(context.Guild.GetChannel(channelId).Name + toAdd);
                        continue;
                    }
                }

                parsedStrings.Add(arg);
            }

            return string.Join(" ", parsedStrings);
        }

        public static string DisplayName(this IGuildUser user) {
            if (user == null) {
                return "unknown user";
            }

            return user.Nickname ?? user.Username;
        }
        
        public static SocketRole GetHighestRole(this SocketGuildUser member, bool requireColor = true)
            => member?.Roles?.Where(x => !requireColor || x.HasColor())?
                .OrderByDescending(x => x.Position)?.FirstOrDefault();

        public static bool HasColor(this IRole role)
            => role.Color.RawValue != 0;

        /// <summary>
        ///     Gets a list of discord users that have been mentioned at the start of the array.
        ///     When a user can't be found, it will stop the parsing
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <param name="remainingArgs">Arguments that are left after parsing</param>
        /// <param name="context"></param>
        /// <param name="serviceProvider">To create a user type reader</param>
        /// <returns></returns>
        public static IEnumerable<IUser> GetDiscordsUsersListFromStrings(this string[] args, out string[] remainingArgs, SocketCommandContext context,
            IServiceProvider serviceProvider) {
            remainingArgs = args.Clone() as string[];
            var result = new List<IUser>();
            var parser = new UserTypeReader<IGuildUser>();

            for (var i = 0; i < args.Length; i++) {
                var arg = args[i];
                var parseResult = parser.ReadAsync(context, arg, serviceProvider).GetAwaiter().GetResult();
                if (parseResult.IsSuccess) {
                    var readerValue = parseResult.Values.FirstOrDefault();
                    if (readerValue.Score >= 0.60f) {
                        result.Add(readerValue.Value as IUser);
                        remainingArgs[i] = "";
                        continue;
                    }
                }

                break;
            }

            remainingArgs = remainingArgs.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            return result;
        }
        
        /// <summary>
        ///     Gets a list of discord users that have been mentioned at the start of the array.
        ///     When a user can't be found, it will skip the string
        /// </summary>
        /// <param name="args">Arguments</param>
        /// <param name="context"></param>
        /// <param name="serviceProvider">To create a user type reader</param>
        /// <returns></returns>
        public static async Task<IEnumerable<IUser>> GetDiscordsUsersListFromStrings<T>(this string[] args, BaseInteractiveContext<T> context) where T : SocketInteraction {
            var result = new List<IUser>();

            foreach (var arg in args) {
                var parseResult = await context.ReadUserAsync<IUser, T>(arg); //parser.ReadAsync(context, arg, serviceProvider).GetAwaiter().GetResult();
                if (!parseResult.IsSuccess) {
                    continue;
                }
            
                var readerValue = parseResult.Values.FirstOrDefault();
                if (readerValue.Score >= 0.60f) {
                    result.Add(readerValue.Value as IUser);
                }
            }
            
            return result;
        }

        // public static CustomPaginatedMessage AddPagingToFooter(this CustomPaginatedMessage message) {
        //     message.EmbedWrapper.Footer ??= new EmbedFooterBuilder();
        //     var whitespace = " ";
        //     if (string.IsNullOrWhiteSpace(message.EmbedWrapper.Footer.Text)) {
        //         whitespace = "";
        //     }
        //
        //     message.EmbedWrapper.Footer.Text += $"{whitespace}{{0}}/{{1}} Pages.";
        //     return message;
        // }
    }
}
