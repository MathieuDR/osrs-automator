using System;
using Newtonsoft.Json;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Requests {
    public class CompetitionRequest {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("metric")]
        public string MetricTypeString => Metric.ToString();

        [JsonIgnore]
        public MetricType Metric { get; set; }
        [JsonProperty("groupVerificationCode")]
        public string VerificationCode { get; set; }
        [JsonProperty("groupId")]
        public int GroupId { get; set; }

        [JsonIgnore]
        public DateTime StartTime { get; set; }
        [JsonIgnore]
        public DateTime EndTime{ get; set; }

        [JsonProperty("startsAt")]
        public string StartTimeUtc {
            get {
                return StartTime.ToUniversalTime()
                    .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            }
        }

        [JsonProperty("endsAt")]
        public string EndTimeUtc {
            get {
                return EndTime.ToUniversalTime()
                    .ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
            }
        }
    }
}