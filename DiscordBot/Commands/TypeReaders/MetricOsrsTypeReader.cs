using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using DiscordBot.Commands.Modules.DiscordCommandArguments;
using DiscordBot.Common.Configuration;
using DiscordBot.Helpers;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Commands.TypeReaders {
    public class MetricOsrsTypeReader : TypeReader {
        private readonly MetricSynonymsConfiguration _config;

        public MetricOsrsTypeReader(MetricSynonymsConfiguration config) {
            _config = config;
        }

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
            var parameters =
                Regex.Matches(input, @"[\""].+?[\""]|[^ ]+").Select(m => m.Value.Replace("\"", "")).ToList();

            if (!parameters.Any()) {
                return Task.FromResult(TypeReaderResult.FromSuccess(null));
            }

            if (parameters.Count > 2) {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount, "Too many arguments!"));
            }

            var result = new MetricArguments();

            foreach (var parameter in parameters) {
                if (parameter.TryParseToMetricType(_config, out var metricType)) {
                    Debug.Assert(metricType != null, nameof(metricType) + " != null");
                    result.MetricType = (MetricType) metricType;
                } else if (string.IsNullOrEmpty(result.Name)) {
                    result.Name = parameter.Replace("\"", "");
                } else {
                    return Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount,
                        $"Wrong errors, Cannot parse all parameters. Ambigious username ({result.Name} & {parameter})"));
                }
            }

            return Task.FromResult(TypeReaderResult.FromSuccess(result));
        }
    }
}
