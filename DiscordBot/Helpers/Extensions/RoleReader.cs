using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Models.Contexts;

namespace DiscordBot.Helpers.Extensions; 

public static class RoleReader {
    public static async Task<TypeReaderResult> ReadRolesAsync<T, TInteraction>(this BaseInteractiveContext<TInteraction> context, string input)
        where T : class, IRole where TInteraction : SocketInteraction {
            
        if(string.IsNullOrWhiteSpace(input)) {
            return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Input emptly");
        }
            
        if (context.Guild != null) {
            var results = new Dictionary<ulong, TypeReaderValue>();
            var roles = context.Guild.Roles;

            //By Mention (1.0)
            if (MentionUtils.TryParseRole(input, out var id)) {
                AddResult(results, context.Guild.GetRole(id) as T, 1.00f);
            }

            //By Id (0.9)
            if (ulong.TryParse(input, NumberStyles.None, CultureInfo.InvariantCulture, out id)) {
                AddResult(results, context.Guild.GetRole(id) as T, 0.90f);
            }

            //By Name (0.8-0.7)
            foreach (var role in roles.Where(x => string.Equals(input, x.Name, StringComparison.OrdinalIgnoreCase))) {
                AddResult(results, role as T, role.Name == input ? 0.80f : 0.70f);
            }
                
            //By Name without @'s (0.69-0.6)
            var inputWithoutAts = input.Replace("@","");
            foreach (var role in roles.Where(x => string.Equals(inputWithoutAts, x.Name, StringComparison.OrdinalIgnoreCase))) {
                AddResult(results, role as T, role.Name == input ? 0.69f : 0.60f);
            }

            if (results.Count > 0) {
                return TypeReaderResult.FromSuccess(results.Values.ToImmutableArray());
            }
        }

        return TypeReaderResult.FromError(CommandError.ObjectNotFound, "Role not found.");
    }

    private static void AddResult(Dictionary<ulong, TypeReaderValue> results, IRole user, float score) {
        if (user != null && !results.ContainsKey(user.Id)) {
            results.Add(user.Id, new TypeReaderValue(user, score));
        }
    }
}