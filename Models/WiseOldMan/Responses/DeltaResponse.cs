using System;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;
using Newtonsoft.Json;

namespace DiscordBotFanatic.Models.WiseOldMan.Responses {
    public class DeltaFullResponse : BaseResponse {
        public DeltaResponse Day { get; set; }
        public DeltaResponse Year { get; set; }
        public DeltaResponse Month { get; set; }
        public DeltaResponse Week { get; set; }
    }

    public class DeltaResponse : BaseResponse {
        public Period Period { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public string Interval { get; set; }

        [JsonProperty("data")]
        public DeltaMetrics Metrics{ get; set; }
    }
}