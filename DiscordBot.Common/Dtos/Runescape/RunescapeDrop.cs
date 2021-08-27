using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Common.Dtos.Runescape {
    public class RunescapeDrop {
        public RunescapeDrop() {
            Item = new RunescapeItem();
            Source = new RunescapeSource();
            Recipient = new Player();
        }

        public RunescapeItem Item { get; set; }
        public int Amount { get; set; }
        public int TotalValue => Item.Value * Amount;
        public int TotalHaValue => Item.HaValue * Amount;
        public RunescapeSource Source { get; set; }
        public float Rarity { get; set; }
        public float RarityPercent => 1f / Rarity;

        public bool IsPet { get; set; }

        public Player Recipient { get; set; }

        public class Player {
            public PlayerType PlayerType { get; set; }
            public string IconUrl { get; set; }
            public string Username { get; set; }
        }
    }
}
