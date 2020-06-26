namespace WiseOldManConnector.Models.API.Responses.Models {
    internal class Metric {
        public int Rank { get; set; }
        public int Experience { get; set; }

        public override string ToString() {
            //return $"L{this.ToLevel()} - {this.FormattedExperience()} exp - Rank {this.FormattedRank()}";
            return $"L {this.ToLevel()}, XP {this.FormattedExperience()}, R {this.FormattedRank()}";
        }
    }
}