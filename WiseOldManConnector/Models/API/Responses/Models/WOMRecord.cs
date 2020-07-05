using System;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Responses.Models {
    internal class WOMRecord:BaseResponse {
        public int Value { get; set; }
        public string Period { get; set; }
        public string Metric { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}