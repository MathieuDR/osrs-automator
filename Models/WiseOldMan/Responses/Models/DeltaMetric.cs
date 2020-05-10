using DiscordBotFanatic.Helpers;

namespace DiscordBotFanatic.Models.WiseOldMan.Responses.Models {
    public class DeltaMetric {
        public Delta Rank { get; set; }
        public Delta Experience { get; set; }

        public override string ToString() {
            //return $"+{this.GainedRanks()}R, (+{this.GainedExperience()}XP)";
            return $"+ {this.LevelGained()}L ({this.GainedExperience()} XP), {this.GainedRanks()} R";
        }
    }
}