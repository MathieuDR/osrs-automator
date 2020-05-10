using System;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Enums;

namespace DiscordBotFanatic.Models.WiseOldMan.Responses.Models {
    public class Record {
        public int Value { get; set; }
        public Period Period { get; set; }
        public MetricType Metric { get; set; }
        public DateTime UpdatedAt { get; set; }

        public override string ToString() {
            return $"Gained {Value.FormatNumber()} experience in the period of a {Period.ToString().ToLowerInvariant()}.";
        }
    }
}