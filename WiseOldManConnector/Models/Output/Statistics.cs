namespace WiseOldManConnector.Models.Output {
    public class Statistics : IBaseConnectorOutput {
        public int MaxedCombatPlayers { get; set; }
        public int MaxedTotalPlayers { get; set; }
        public int Maxed200MExpPlayers { get; set; }

        public Snapshot AverageStats { get; set; }
    }
}
