using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using DiscordBotFanatic.Modules.DiscordCommandArguments;

namespace DiscordBotFanatic.TypeReaders {
    public class BaseArgumentsTypeReader : TypeReader {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
            List<string> parameters =
                Regex.Matches(input, @"[\""].+?[\""]|[^ ]+").Select(m => m.Value.Replace("\"", "")).ToList();

            if (!parameters.Any()) {
                return Task.FromResult(TypeReaderResult.FromSuccess(null));
            }

            if (parameters.Count > 1) {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount, $"Too many arguments!"));
            }

            var result = new MetricArguments();

            foreach (string parameter in parameters) {
                result.Name = parameter.Replace("\"", "");
            }

            return Task.FromResult(TypeReaderResult.FromSuccess(result));
        }
    }
}