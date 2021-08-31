using System.Collections.Generic;

namespace WiseOldManConnector.Models.Output {
    public class CompetitionParticipant : ILeaderboardMember {
        public Delta CompetitionDelta { get; set; }
        public List<HistoryItem> History { get; set; }
        public Player Player { get; set; }

        public double Value => CompetitionDelta.Gained;
    }
}
