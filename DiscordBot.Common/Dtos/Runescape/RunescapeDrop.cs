using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Common.Dtos.Runescape; 

public record RunescapeDrop {
    public RunescapeDrop() {
        Item = new RunescapeItem();
        Source = new RunescapeSource();
        Recipient = new Player();
    }

    public RunescapeDrop(string image) {
        Image = image;
    }

    public RunescapeItem Item { get; init; }
    public int Amount { get; set; }
    public int TotalValue => Item.Value * Amount;
    public int TotalHaValue => Item.HaValue * Amount;
    public RunescapeSource Source { get; init; }
    public float Rarity { get; init; }
    public float RarityPercent => 1f / Rarity;

    public bool IsPet { get; init; }

    public Player Recipient { get; init; }

    public string Image { get; init; }

    public record Player {
        public PlayerType PlayerType { get; init; }
        public string IconUrl { get; init; }
        public string Username { get; init; }
    }
}