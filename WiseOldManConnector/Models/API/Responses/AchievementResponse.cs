﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WiseOldManConnector.Helpers.JsonConverters;

namespace WiseOldManConnector.Models.API.Responses {
    [JsonConverter(typeof(AchievementsConverter))]
    internal class AchievementResponse : BaseResponse {
        public List<WOMAchievement> Achievements { get; set; }
    }
}