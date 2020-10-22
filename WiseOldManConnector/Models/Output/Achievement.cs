﻿using System;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.Output {
    public class Achievement{
        public int PlayerId { get; set; }
        public MetricType Metric { get; set; }
        public string Title { get; set; }
        public long Threshold { get; set; }
        public DateTime? AchievedAt { get; set; }
        public Boolean IsMissing { get; set; }
    }
}