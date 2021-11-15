using System.Collections.Immutable;
using System.Globalization;
using Discord.Commands;

namespace DiscordBot.Helpers.Extensions;

public static class UserReader {
    public static async Task<TypeReaderResult> ReadUserAsync<T, TInteraction>(this BaseInteractiveContext<TInteraction> context, string input)
        where T : class, IUser where TInteraction : SocketInteraction {
        var results = new Dictionary<ulong, TypeReaderValue>();
        var channelUsersTask = context.Channel.GetUsersAsync(CacheMode.CacheOnly).Flatten().ToListAsync(); // it's better
        var guildUsersTask = new ValueTask<List<IGuildUser>>();

        if (context.Guild != null) {
            guildUsersTask = context.Guild.GetUsersAsync().Flatten().ToListAsync();
        }

        if (string.IsNullOrWhiteSpace(input)) {
            return TypeReaderResult.FromError(CommandError.ObjectNotFound, "User not found.");
        }

        //By Mention (1.0)
        if (MentionUtils.TryParseUser(input, out var id)) {
            if (context.Guild != null) {
                AddResult(results, context.Guild.GetUser(id) as T, 1.00f);
            } else {
                AddResult(results, await context.Channel.GetUserAsync(id, CacheMode.CacheOnly).ConfigureAwait(false) as T, 1.00f);
            }
        }

        //By Id (0.9)
        if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out id)) {
            if (context.Guild != null) {
                AddResult(results, context.Guild.GetUser(id) as T, 0.90f);
            } else {
                AddResult(results, await context.Channel.GetUserAsync(id, CacheMode.CacheOnly).ConfigureAwait(false) as T, 0.90f);
            }
        }

        //By Username + Discriminator (0.7-0.85)
        var channelUsers = await channelUsersTask;
        var guildUsers = await guildUsersTask ?? new List<IGuildUser>();
        var index = input.LastIndexOf('#');
        if (index >= 0) {
            var username = input.Substring(0, index);
            if (ushort.TryParse(input.Substring(index + 1), out var discriminator)) {
                var channelUser = channelUsers.FirstOrDefault(x => x.DiscriminatorValue == discriminator &&
                                                                   string.Equals(username, x.Username, StringComparison.OrdinalIgnoreCase));
                AddResult(results, channelUser as T, channelUser?.Username == username ? 0.80f : 0.70f);


                var guildUser = guildUsers.FirstOrDefault(x => x.DiscriminatorValue == discriminator &&
                                                               string.Equals(username, x.Username, StringComparison.OrdinalIgnoreCase));
                AddResult(results, guildUser as T, guildUser?.Username == username ? 0.80f : 0.70f);
            }
        }

        //By Username (0.5-0.6)
        {
            foreach (var channelUser in channelUsers.Where(x => string.Equals(input, x.Username, StringComparison.OrdinalIgnoreCase))) {
                AddResult(results, channelUser as T, channelUser.Username == input ? 0.60f : 0.50f);
            }

            foreach (var guildUser in guildUsers.Where(x => string.Equals(input, x.Username, StringComparison.OrdinalIgnoreCase))) {
                AddResult(results, guildUser as T, guildUser.Username == input ? 0.60f : 0.50f);
            }
        }

        //By Nickname (0.5-0.6)
        {
            foreach (var channelUser in channelUsers.Where(x =>
                         string.Equals(input, (x as IGuildUser)?.Nickname, StringComparison.OrdinalIgnoreCase))) {
                AddResult(results, channelUser as T, ((IGuildUser)channelUser).Nickname == input ? 0.60f : 0.50f);
            }


            foreach (var guildUser in guildUsers.Where(x => string.Equals(input, x.Nickname, StringComparison.OrdinalIgnoreCase))) {
                AddResult(results, guildUser as T, guildUser.Nickname == input ? 0.60f : 0.50f);
            }
        }

        if (results.Count > 0) {
            return TypeReaderResult.FromSuccess(results.Values.ToImmutableArray());
        }

        return TypeReaderResult.FromError(CommandError.ObjectNotFound, "User not found.");
    }

    private static void AddResult(Dictionary<ulong, TypeReaderValue> results, IUser user, float score) {
        if (user != null && !results.ContainsKey(user.Id)) {
            results.Add(user.Id, new TypeReaderValue(user, score));
        }
    }
}
