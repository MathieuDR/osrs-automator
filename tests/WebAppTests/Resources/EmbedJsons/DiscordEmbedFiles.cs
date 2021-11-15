using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WebAppTests.Resources.EmbedJsons;

public class DiscordEmbedFiles {
    private static IEnumerable<File> Files =>
        new List<File> {
            new(@"Resources/EmbedJsons/discordAutomatorJson.json", "ErkendRserke", PlayerType.Regular, "Coins", 5, 5, 0, "Goblin", 5, 42.666668f,
                @"https://oldschool.runescape.wiki/w/Coins", @"https://oldschool.runescape.wiki/w/Goblin#Level_5",
                @"https://oldschool.runescape.wiki/images/4/44/Coins_1.png?a3fa8"),
            new(@"Resources/EmbedJsons/discordAutomatorJson2.json", "ErkendRserke", PlayerType.Regular, "Bones", 1, 86, 0, "Goblin", 5, 1.0f,
                @"https://oldschool.runescape.wiki/w/Bones", @"https://oldschool.runescape.wiki/w/Goblin#Level_5",
                @"https://oldschool.runescape.wiki/images/5/5e/Bones.png?3fbd5"),
            new(@"Resources/EmbedJsons/discordAutomatorJson3.json", "arstiE", PlayerType.Regular, "Triton blunders", 3, 900, 123, "Kree'bra", 213,
                59f, @"https://oldschool.runescape.wiki/w/Bones", @"https://oldschool.runescape.wiki/w/Goblin#Level_5",
                @"https://oldschool.runescape.wiki/images/5/5e/Bones.png?3fbd5"),
            new(@"Resources/EmbedJsons/discordAutomatorJson4.json", "arstiE", PlayerType.Regular, "Triton blunders", 3, 900, 123, "Kree'bra", null
                , 59f, @"https://oldschool.runescape.wiki/w/Bones", @"https://oldschool.runescape.wiki/w/Goblin#Level_5",
                @"https://oldschool.runescape.wiki/images/5/5e/Bones.png?3fbd5"),
            new(@"Resources/EmbedJsons/discordAutomatorJson_ironman.json", "wagen", PlayerType.IronMan, "Bones", 1, 92, 0, "Al-Kharid warrior", 9,
                1.0f, @"https://oldschool.runescape.wiki/w/Bones", @"https://oldschool.runescape.wiki/w/Al-Kharid_warrior",
                @"https://oldschool.runescape.wiki/images/5/5e/Bones.png?3fbd5"),
            new(@"Resources/EmbedJsons/discordAutomatorJson_hc.json", "wagen", PlayerType.HardcoreIronMan, "Bones", 1, 92, 0, "Al-Kharid warrior",
                9, 1.0f, @"https://oldschool.runescape.wiki/w/Bones", @"https://oldschool.runescape.wiki/w/Al-Kharid_warrior",
                @"https://oldschool.runescape.wiki/images/5/5e/Bones.png?3fbd5"),
            new(@"Resources/EmbedJsons/discordAutomatorJson_uim.json", "wagen", PlayerType.UltimateIronMan, "Bones", 1, 92, 0, "Al-Kharid warrior"
                , 9, 1.0f, @"https://oldschool.runescape.wiki/w/Bones", @"https://oldschool.runescape.wiki/w/Al-Kharid_warrior",
                @"https://oldschool.runescape.wiki/images/5/5e/Bones.png?3fbd5")
        };

    public static IEnumerable<object[]> AllFiles => Files.Select(x => new[] { x.Path });
    public static string FirstFile => Files.Select(x => x.Path).FirstOrDefault();
    public static string FirstImFile => Files.Where(x => x.PlayerType == PlayerType.IronMan).Select(x => x.Path).FirstOrDefault();
    public static IEnumerable<object[]> WithPlayerTypes => Files.Select(x => new object[] { x.Path, x.PlayerType });
    public static IEnumerable<object[]> WithQuantities => Files.Select(x => new object[] { x.Path, x.Amount });
    public static IEnumerable<object[]> WithNames => Files.Select(x => new object[] { x.Path, x.Player });
    public static IEnumerable<object[]> WithValues => Files.Select(x => new object[] { x.Path, x.Amount, x.Value, x.HaValue });

    public static IEnumerable<object[]> WithSources => Files.Select(x => new object[] { x.Path, x.Source, x.Level, x.SourceUrl });
    public static IEnumerable<object[]> WithItem => Files.Select(x => new object[] { x.Path, x.Item, x.ItemUrl, x.IconUrl });
    public static IEnumerable<object[]> WithRarity => Files.Select(x => new object[] { x.Path, x.Rarity });

    public class File {
        public File(string path, string player, PlayerType type, string item, int amount, float value, int haValue,
            string source, int? level, float rarity, string itemUrl,
            string sourceUrl, string iconUrl, bool isCorrect = true) {
            Path = path;
            PlayerType = type;
            Amount = amount;
            Level = level;
            Rarity = rarity;
            Value = value;
            Item = item;
            ItemUrl = itemUrl;
            Source = source;
            SourceUrl = sourceUrl;
            IconUrl = iconUrl;
            Player = player;
            HaValue = haValue;
            IsCorrect = isCorrect;
        }

        public string Path { get; }
        public PlayerType PlayerType { get; }
        public int Amount { get; }
        public int? Level { get; }
        public float Rarity { get; }
        public float Value { get; }
        public string Item { get; }
        public string ItemUrl { get; }
        public string Source { get; }
        public string SourceUrl { get; }
        public string IconUrl { get; }
        public string Player { get; }
        public int HaValue { get; }
        public bool IsCorrect { get; }
    }
}
