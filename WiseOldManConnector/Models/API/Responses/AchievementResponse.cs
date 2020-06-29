using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WiseOldManConnector.Helpers.JsonConverters;
using WiseOldManConnector.Models.API.Responses.Models;

namespace WiseOldManConnector.Models.API.Responses {
    [JsonConverter(typeof(AchievementsConverter))]
    internal class AchievementResponse : BaseResponse {
        public List<Achievement> Achievements { get; set; }
    }
}