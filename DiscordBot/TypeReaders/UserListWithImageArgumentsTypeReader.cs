using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBot.Helpers;
using DiscordBot.Modules.DiscordCommandArguments;

namespace DiscordBot.TypeReaders {
    public class UserListWithImageArgumentsTypeReader : TypeReader {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
            var parameters = input.ToCollectionOfParameters().ToList();

            var usersCollection = context.Channel.GetUsersAsync();
            var result = new UserListWithImageArguments();

            var userResultList = new List<IUser>();

            if (context.Message.Attachments.Count == 1) {
                result.ImageUrl = context.Message.Attachments.FirstOrDefault()?.Url;
            } else if (context.Message.Attachments.Any()) {
                return await Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount,
                    $"Maximum one attachment ({context.Message.Attachments.Count})"));
            }

            if (usersCollection == null) {
                return await Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed,
                    $"No users in channel. Cannot parse user mentions"));
            }

            if (parameters.Count != parameters.Distinct().Count()) {
                return await Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed,
                    $"Repeating values in arguments."));
            }

            foreach (string parameter in parameters) {
                var cts = new CancellationTokenSource();
                if (MentionUtils.TryParseUser(parameter, out var userId)) {
                    // ReSharper disable once PossibleMultipleEnumeration
                    await foreach (var users in usersCollection.WithCancellation(cts.Token)) {
                        foreach (var user in users) {
                            if (user.Id != userId) {
                                continue;
                            }

                            if (user.IsBot) {
                                return await Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed,
                                    $"User is a bot ({parameter})"));
                            }

                            userResultList.Add(user);
                            cts.Cancel();
                        }
                    }
                } else if (string.IsNullOrEmpty(result.ImageUrl)) {
                    if (parameter.IsValidUrl()) {
                        result.ImageUrl = parameter;
                    } else {
                        return await Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed,
                            $"Cannot parse arguments. Not correct url format ({parameter})"));
                    }
                } else {
                    return await Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount,
                        $"Cannot parse arguments. Ambiguous imageUrl ({result.ImageUrl} & {parameter})"));
                }
            }

            result.Users = userResultList;

            if (!result.Users.Any()) {
                return await Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount,
                    $"Need at least one user mentioned"));
            }

            if (string.IsNullOrEmpty(result.ImageUrl)) {
                return await Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount,
                    $"Need an image url or attachment."));
            }

            return await Task.FromResult(TypeReaderResult.FromSuccess(result));
        }
    }
}