using System.Collections.Generic;
using Newtonsoft.Json;
using WiseOldManConnector.Models.API.Responses.Models;

namespace WiseOldManConnector.Models.API.Responses {
    internal class RecordResponse : BaseResponse{

        [JsonProperty("day")]
        public List<WOMRecord> Day { get; set; }

        [JsonProperty("week")]
        public List<WOMRecord> Week { get; set; }

        [JsonProperty("month")]
        public List<WOMRecord> Month { get; set; }

        [JsonProperty("year")]
        public List<WOMRecord> Year { get; set; }

    }
}