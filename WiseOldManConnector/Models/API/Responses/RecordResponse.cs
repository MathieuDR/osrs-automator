using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WiseOldManConnector.Models.API.Responses.Models;

namespace WiseOldManConnector.Models.API.Responses {
    [JsonConverter(typeof(RecordsConverter))]
    public class RecordResponse : BaseResponse{
        public List<Record> Records { get; set; }

        private List<RecordInfo> _recordInfos;

        public List<RecordInfo> RecordInfos {
            get {
                return _recordInfos ??= Records.Select(x => x.ToRecordInfo()).ToList().RemoveEmpty();
            }
        }
    }
}