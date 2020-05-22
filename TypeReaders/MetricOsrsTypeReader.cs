using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Modules.DiscordCommandArguments;

namespace DiscordBotFanatic.TypeReaders {
    public class MetricOsrsTypeReader : TypeReader {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input,
            IServiceProvider services) {
            List<string> parameters = Regex.Matches(input, @"[\""].+?[\""]|[^ ]+")
                .Select(m => m.Value.Replace("\"",""))
                .ToList();

            if (!parameters.Any()) {
                return Task.FromResult(TypeReaderResult.FromSuccess(null));
            }

            if (parameters.Count > 2) {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount, $"Too many arguments!"));
            }

            var result = new MetricArguments();

            foreach (string parameter in parameters) {
                if (Enum.TryParse(typeof(MetricType), parameter, true, out object metricType)) {
                    Debug.Assert(metricType != null, nameof(metricType) + " != null");
                    result.MetricType = (MetricType) metricType;
                }
                else if (string.IsNullOrEmpty(result.Name)) {
                    result.Name = parameter.Replace("\"","");
                }
                else {
                    return Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount,
                        $"Wrong errors, Cannot parse all parameters. Ambigious username ({result.Name} & {parameter})"));
                }
            }

            return Task.FromResult(TypeReaderResult.FromSuccess(result));
        }
    }
}