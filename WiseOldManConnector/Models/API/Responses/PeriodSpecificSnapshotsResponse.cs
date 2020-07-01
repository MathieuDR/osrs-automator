using System.Collections.Generic;
using Newtonsoft.Json;
using WiseOldManConnector.Helpers.JsonConverters;
using WiseOldManConnector.Models.API.Responses.Models;

namespace WiseOldManConnector.Models.API.Responses {
    [JsonConverter(typeof(SnapshotsConverter))]
    internal class PeriodSpecificSnapshotsResponse : BaseResponse {
        public List<Snapshot> Snapshots { get; set; }
    }
}