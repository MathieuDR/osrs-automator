using System;

namespace WiseOldManConnector.Models.Output {
    public class DeltaMember : ILeaderboardMember {
        public Delta Delta { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public Player Player { get; set; }
        public double Value => Delta.Gained;
    }
}
