using System;

namespace WiseOldManConnector.Models.Output {
    public class DeltaMember {
        public Player Player { get; set; }
        public Delta Delta { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
}
