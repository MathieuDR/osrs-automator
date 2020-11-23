using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Configuration;
using DiscordBotFanatic.Modules.DiscordCommandArguments;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBotFanatic.TypeReaders {
    public class PeriodAndMetricOsrsTypeReader : TypeReader {
        private readonly MetricSynonymsConfiguration _config;

        public PeriodAndMetricOsrsTypeReader(MetricSynonymsConfiguration config) {
            _config = config;
        }

        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
            List<string> parameters = input.ToCollectionOfParameters().ToList();

            if (!parameters.Any()) {
                return Task.FromResult(TypeReaderResult.FromSuccess(null));
            }

            if (parameters.Count > 3) {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.BadArgCount, $"Too many arguments!"));
            }

            var result = new PeriodAndMetricArguments();

            foreach (string parameter in parameters) {
                if (Enum.TryParse(typeof(Period), parameter, true, out object period)) {
                    Debug.Assert(period != null, nameof(period) + " != null");
                    result.Period = (Period) period;
                } else if (parameter.TryParseToMetricType(_config, out object metricType)) {
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