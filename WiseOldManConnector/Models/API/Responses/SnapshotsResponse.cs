using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WiseOldManConnector.Models.API.Responses.Models;

namespace WiseOldManConnector.Models.API.Responses {
    internal class SnapshotsResponse : BaseResponse {
        [JsonProperty("day")]
        public List<WOMSnapshot> Day { get; set; }

        [JsonProperty("week")]
        public List<WOMSnapshot> Week { get; set; }

        [JsonProperty("month")]
        public List<WOMSnapshot> Month { get; set; }

        [JsonProperty("year")]
        public List<WOMSnapshot> Year { get; set; }

    }
}