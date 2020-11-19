using System;

namespace WiseOldManConnector.Models.Output {
    public class HistoryItem {
        public DateTimeOffset DateTime { get; set; }
        public int Value { get; set; }
    }
}