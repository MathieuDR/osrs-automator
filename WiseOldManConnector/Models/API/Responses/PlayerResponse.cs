using System;
using System.Collections.Generic;
using System.Linq;
using WiseOldManConnector.Models.API.Responses.Models;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Responses {
    internal class PlayerResponse : BaseResponse {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Type { get; set; }
        public DateTime LastImportedAt { get; set; }
        public DateTime RegisteredAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public Snapshot LatestSnapshot { get; set; }

        public List<MetricInfo> MetricInfoList {
            get { return this.LatestSnapshot.MetricInfoList; }
        }

        public MetricInfo MetricForType(MetricType type) {
            return MetricInfoList.SingleOrDefault(x => x.Type == type);
        }
    }
}