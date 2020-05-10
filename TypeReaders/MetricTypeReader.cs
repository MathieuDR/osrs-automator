using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord.Commands;
using DiscordBotFanatic.Models.Enums;

namespace DiscordBotFanatic.TypeReaders {
    public class MetricTypeReader : TypeReader {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
            if (int.TryParse(input, out int value)) {
                if (Enum.IsDefined(typeof(MetricType), value)) {
                    return Task.FromResult(TypeReaderResult.FromSuccess((MetricType) value));
                }
            }else if (Enum.TryParse(typeof(MetricType), input, true, out object result)) {
                Debug.Assert(result != null, nameof(result) + " != null");
                return Task.FromResult(TypeReaderResult.FromSuccess((MetricType) result));
            }

            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed,
                $"Input ({input}) could not be parsed as a {nameof(MetricType)}."));
        }
    }
}