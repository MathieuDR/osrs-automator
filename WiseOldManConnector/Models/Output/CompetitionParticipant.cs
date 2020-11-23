using System.Collections.Generic;

namespace WiseOldManConnector.Models.Output {
    public class CompetitionParticipant {
        public Player Player { get; set; }
        public Delta CompetitionDelta { get; set; }
        public List<HistoryItem> History { get; set; }
    }
}