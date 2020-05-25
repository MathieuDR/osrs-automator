﻿using System;
using System.Collections.Generic;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Models.WiseOldMan.Responses.Models;
using Newtonsoft.Json;

namespace DiscordBotFanatic.Models.WiseOldMan.Responses {
    public class CreateGroupCompetitionResult:BaseResponse
    {

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("metric")]
        public string Metric { get; set; }

        [JsonIgnore]
        public MetricType MetricType{
            get {
                if (Enum.TryParse(typeof(MetricType), Metric, true, out object value)) {
                    return (MetricType)value;
                }

                return (MetricType) 0;
            }
        }

        [JsonProperty("verificationCode")]
        public string VerificationCode { get; set; }

        [JsonProperty("startsAt")]
        public DateTime StartsAt { get; set; }

        [JsonProperty("endsAt")]
        public DateTime EndsAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("participants")]
        public IList<Participant> Participants { get; set; }
    }

}