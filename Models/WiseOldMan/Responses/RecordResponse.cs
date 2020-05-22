using System.Collections.Generic;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Helpers.JsonConverters;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;
using Newtonsoft.Json;

namespace DiscordBotFanatic.Models.WiseOldMan.Responses {
    [JsonConverter(typeof(RecordsConverter))]
    public class RecordResponse : BaseResponse{

        public List<Record> Records { get; set; }
    }
}