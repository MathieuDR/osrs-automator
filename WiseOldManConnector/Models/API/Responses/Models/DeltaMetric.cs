namespace WiseOldManConnector.Models.API.Responses.Models {
    public class DeltaMetric {
        public Delta Rank { get; set; }
        public Delta Experience { get; set; }
        public Delta Score { get; set; }
        public Delta Kills{ get; set; }

        public override string ToString() {
            if (Rank == null || Rank.End == -1) {
                return "-1";
            }

            //return $"+{this.GainedRanks()}R, (+{this.GainedExperience()}XP)";
            if (Experience != null) {
                return $"+{this.LevelGained()} LVL('s) ({this.GainedExperience()} XP), {this.GainedRanks()} Ranks ({this.Rank.End})";
            }

            if (Score != null) {
                return $"+{Score.Gained} ({Score.End}), {this.GainedRanks()} Ranks ({this.Rank.End})";
            }

            return "-1";
        }
    }
}