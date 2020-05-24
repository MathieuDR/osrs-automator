using System.Collections.Generic;
using System.Linq;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Helpers.JsonConverters;
using DiscordBotFanatic.Models.WiseOldMan.Cleaned;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;
using Newtonsoft.Json;

namespace DiscordBotFanatic.Models.WiseOldMan.Responses {
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