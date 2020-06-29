using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WiseOldManConnector.Helpers.JsonConverters;
using WiseOldManConnector.Models.API.Responses.Models;

namespace WiseOldManConnector.Models.API.Responses {
    [JsonConverter(typeof(RecordsConverter))]
    internal class RecordResponse : BaseResponse{
        public List<Record> Records { get; set; }
    }
}