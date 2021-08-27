using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using DiscordBot.Modules.DiscordCommandArguments;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.TypeReaders {
    public class PeriodOsrsTypeReader : TypeReader {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
            var parameters =
                Regex.Matches(input, @"[\""].+?[\""]|[^ ]+").Select(m => m.Value.Replace("\"", "")).ToList();

            if (!parameters.Any()) {
                return Task.FromResult(TypeReaderResult.FromSuccess(null));
            }

            if (parameters.Count > 2) {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount, "Too many arguments!"));
            }

            var result = new PeriodArguments();

            foreach (var parameter in parameters) {
                if (Enum.TryParse(typeof(Period), parameter, true, out var period)) {
                    Debug.Assert(period != null, nameof(period) + " != null");
                    result.Period = (Period) period;
                } else if (string.IsNullOrEmpty(result.Name)) {
                    result.Name = parameter;
                } else {
                    return Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount,
                        $"Wrong errors, Cannot parse all parameters. Ambigious username ({result.Name} & {parameter})"));
                }
            }

            return Task.FromResult(TypeReaderResult.FromSuccess(result));
        }
    }
}
