using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WiseOldManConnector.Models.API.Responses.Models;

namespace WiseOldManConnector.Models.API.Responses {
    internal class SnapshotsResponse : BaseResponse {
        [JsonProperty("day")]
        public List<Snapshot> Day { get; set; }

        [JsonProperty("week")]
        public List<Snapshot> Week { get; set; }

        [JsonProperty("month")]
        public List<Snapshot> Month { get; set; }

        [JsonProperty("year")]
        public List<Snapshot> Year { get; set; }

    }
}