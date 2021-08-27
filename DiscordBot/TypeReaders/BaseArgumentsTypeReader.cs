using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using DiscordBot.Modules.DiscordCommandArguments;

namespace DiscordBot.TypeReaders {
    public class BaseArgumentsTypeReader : TypeReader {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
            var parameters =
                Regex.Matches(input, @"[\""].+?[\""]|[^ ]+").Select(m => m.Value.Replace("\"", "")).ToList();

            if (!parameters.Any()) {
                return Task.FromResult(TypeReaderResult.FromSuccess(null));
            }

            if (parameters.Count > 1) {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount, "Too many arguments!"));
            }

            var result = new MetricArguments();

            foreach (var parameter in parameters) {
                result.Name = parameter.Replace("\"", "");
            }

            return Task.FromResult(TypeReaderResult.FromSuccess(result));
        }
    }
}
