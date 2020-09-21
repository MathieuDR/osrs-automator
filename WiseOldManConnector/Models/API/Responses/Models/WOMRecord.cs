using System;
using System.ComponentModel;
using Newtonsoft.Json;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Responses.Models {
    internal class WOMRecord : BaseResponse {

        [JsonProperty("value")]
        public int Value { get; set; }
        [JsonProperty("period")]
        public string Period { get; set; }

        [JsonProperty("metric")]
        public string MetricType { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("id")]
        public int Id{ get; set; }

        [JsonProperty("playerId")]
        public int PlayerId{ get; set; }
        
        [JsonProperty("player")]
        public PlayerResponse Player{ get; set; }

        //[JsonProperty("type")]
        //public string PlayerType { get; set; }

        //[JsonProperty("displayName")]
        //public string DisplayName { get; set; }
    }
}