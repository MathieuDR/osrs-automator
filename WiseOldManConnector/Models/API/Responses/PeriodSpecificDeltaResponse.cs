using System;
using Newtonsoft.Json;
using WiseOldManConnector.Models.API.Responses.Models;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Responses {
    internal class DeltaFullResponse : BaseResponse { // TODO Rename
        public DeltaResponse Day { get; set; }
        public DeltaResponse Year { get; set; }
        public DeltaResponse Month { get; set; }
        public DeltaResponse Week { get; set; }
    }

    internal class DeltaResponse : BaseResponse { // TODO Rename
        public Period Period { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public string Interval { get; set; }

        [JsonProperty("data")]
        public DeltaMetrics Metrics{ get; set; }
    }
}