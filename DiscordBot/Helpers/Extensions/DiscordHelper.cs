using Discord.Commands;

namespace DiscordBot.Helpers.Extensions; 

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
    ///     When a user can't be found, it will skip the string
    /// </summary>
    /// <param name="args">Arguments</param>
    /// <param name="context"></param>
    /// <param name="serviceProvider">To create a user type reader</param>
    /// <returns></returns>
    public static async Task<(IEnumerable<IUser> users, string[] remainingArgs)> GetDiscordUsersListFromStrings<T>(this string[] args, BaseInteractiveContext<T> context) where T : SocketInteraction {
        var result = new List<IUser>();
        var remainingArguments = new List<string>();

        foreach (var arg in args) {
            var parseResult = await context.ReadUserAsync<IUser, T>(arg); //parser.ReadAsync(context, arg, serviceProvider).GetAwaiter().GetResult();
            if (!parseResult.IsSuccess) {
                remainingArguments.Add(arg);
                continue;
            }
            
            var readerValue = parseResult.Values.FirstOrDefault();
            if (readerValue.Score >= 0.60f) {
                result.Add(readerValue.Value as IUser);
            }else {
                remainingArguments.Add(arg);
            }
        }
            
        return (result, remainingArguments.ToArray());
    }
        
    /// <summary>
    ///     Gets a list of discord roles that have been mentioned at the start of the array.
    ///     When a role can't be found, it will skip the string
    /// </summary>
    /// <param name="args">array to check</param>
    /// <param name="context">Context</param>
    /// <typeparam name="T">Context type</typeparam>
    /// <returns>Roles and arguments it could not parse</returns>
    public static async Task<(IEnumerable<IRole> roles, string[] remainingArgs)> GetDiscordRolesListFromStrings<T>(this string[] args, BaseInteractiveContext<T> context) where T : SocketInteraction {
        var result = new List<IRole>();
        var remainingArguments = new List<string>();
            
        foreach (var arg in args) {
            var parseResult = await context.ReadRolesAsync<IRole, T>(arg); //parser.ReadAsync(context, arg, serviceProvider).GetAwaiter().GetResult();
            if (!parseResult.IsSuccess) {
                remainingArguments.Add(arg);
                continue;
            }
            
            var readerValue = parseResult.Values.FirstOrDefault();
            result.Add(readerValue.Value as IRole);
        }
            
        return (result, remainingArguments.ToArray());
    }
        
    /// <summary>
    ///     Gets a list of discord users and roles that have been mentioned at the start of the array.
    ///     When a user or role cannot be found, it will skip the string
    /// </summary>
    /// <param name="args">array to check</param>
    /// <param name="context">Context</param>
    /// <typeparam name="T">Context type</typeparam>
    /// <returns>Roles and arguments it could not parse</returns>
    public static async Task<(IEnumerable<IUser> users, IEnumerable<IRole> roles, string[] remainingArgs)> GetDiscordUsersAndRolesListFromStrings<T>(this string[] args, BaseInteractiveContext<T> context) where T : SocketInteraction {
        var (roles, remainingRolesArgs) = await args.GetDiscordRolesListFromStrings(context);
        var (users, remainingUserArgs) = (await remainingRolesArgs.GetDiscordUsersListFromStrings(context));
        return (users, roles, remainingUserArgs);
    }

    /// <summary>
    /// Returns all users from the argument, including found roles
    /// </summary>
    /// <param name="args"></param>
    /// <param name="context"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async Task<(IEnumerable<IUser> users, string[] remainingArgs)> GetUsersListFromStringWithRoles<T>(this string[] args,
        BaseInteractiveContext<T> context) where T : SocketInteraction {
        var (usersMentions, roles, remainingRolesArgs) = await args.GetDiscordUsersAndRolesListFromStrings(context);
        var users = usersMentions.ToList();
            
        foreach (IRole role in roles) {
            users.AddRange(role.GetUsersFromRole(context));
        }
        return (users.Distinct(), remainingRolesArgs);
    }

    public static string ToChannel(this ulong id) {
        return $"<#{id}>";
    }
        
    public static string ToRole(this ulong id) {
        return $"<@&{id}>";
    }
        
    public static string ToUser(this ulong id) {
        return $"<@{id}>";
    }

        
        
    /// <summary>
    /// Get all users in role
    /// </summary>
    /// <param name="role"></param>
    /// <param name="context"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<IUser> GetUsersFromRole<T>(this IRole role, BaseInteractiveContext<T> context) where T : SocketInteraction {
        return role switch {
            SocketRole socketRole => socketRole.Members,
            RestRole restRole => throw new NotImplementedException("RestRole not implemented"),
            _ => throw new ArgumentException("Role kind not found")
        };
    }
}