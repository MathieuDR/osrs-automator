using System.Runtime.Serialization;

namespace WiseOldManConnector.Models.WiseOldMan.Enums {
    public enum MetricType {
        [EnumMember(Value = "overall")]
        Overall = 1,
        [EnumMember(Value = "attack")]
        Attack,
        [EnumMember(Value = "defence")]
        Defence,
        [EnumMember(Value = "strength")]
        Strength,
        [EnumMember(Value = "hitpoints")]
        Hitpoints,
        [EnumMember(Value = "ranged")]
        Ranged,
        [EnumMember(Value = "prayer")]
        Prayer,
        [EnumMember(Value = "magic")]
        Magic,
        [EnumMember(Value = "cooking")]
        Cooking,
        [EnumMember(Value = "woodcutting")]
        Woodcutting,
        [EnumMember(Value = "fletching")]
        Fletching,
        [EnumMember(Value = "fishing")]
        Fishing,
        [EnumMember(Value = "firemaking")]
        Firemaking,
        [EnumMember(Value = "crafting")]
        Crafting,
        [EnumMember(Value = "smithing")]
        Smithing,
        [EnumMember(Value = "mining")]
        Mining,
        [EnumMember(Value = "herblore")]
        Herblore,
        [EnumMember(Value = "agility")]
        Agility,
        [EnumMember(Value = "thieving")]
        Thieving,
        [EnumMember(Value = "slayer")]
        Slayer,
        [EnumMember(Value = "farming")]
        Farming,

        [EnumMember(Value = "runecrafting")]
        Runecrafting,

        [EnumMember(Value = "hunter")]
        Hunter,

        [EnumMember(Value = "construction")]
        Construction,

        [EnumMember(Value = "league_points")]
        LeaguePoints = 50,

        [EnumMember(Value = "bounty_hunter_hunter")]
        BountyHunterHunter,

        [EnumMember(Value = "bounty_hunter_rogue")]
        BountyHunterRogue,

        [EnumMember(Value = "clue_scrolls_all")]
        ClueScrollsAll,

        [EnumMember(Value = "clue_scrolls_beginner")]
        ClueScrollsBeginner,

        [EnumMember(Value = "clue_scrolls_easy")]
        ClueScrollsEasy,

        [EnumMember(Value = "clue_scrolls_medium")]
        ClueScrollsMedium,

        [EnumMember(Value = "clue_scrolls_hard")]
        ClueScrollsHard,

        [EnumMember(Value = "clue_scrolls_elite")]
        ClueScrollsElite,

        [EnumMember(Value = "clue_scrolls_master")]
        ClueScrollsMaster,

        [EnumMember(Value = "last_man_standing")]
        LastManStanding,
        [EnumMember(Value = "soul_wars_zeal")]
        SoulWarsZeal,

        [EnumMember(Value = "abyssal_sire")]
        AbyssalSire,

        [EnumMember(Value = "alchemical_hydra")]
        AlchemicalHydra,

        [EnumMember(Value = "barrows_chests")]
        BarrowsChests,

        [EnumMember(Value = "bryophyta")]
        Bryophyta,

        [EnumMember(Value = "callisto")]
        Callisto,

        [EnumMember(Value = "cerberus")]
        Cerberus,

        [EnumMember(Value = "chambers_of_xeric")]
        ChambersOfXeric,

        [EnumMember(Value = "chambers_of_xeric_challenge_mode")]
        ChambersOfXericChallengeMode,

        [EnumMember(Value = "chaos_elemental")]
        ChaosElemental,

        [EnumMember(Value = "chaos_fanatic")]
        ChaosFanatic,

        [EnumMember(Value = "commander_zilyana")]
        CommanderZilyana,

        [EnumMember(Value = "corporeal_beast")]
        CorporealBeast,

        [EnumMember(Value = "crazy_archaeologist")]
        CrazyArchaeologist,

        [EnumMember(Value = "dagannoth_prime")]
        DagannothPrime,

        [EnumMember(Value = "dagannoth_rex")]
        DagannothRex,

        [EnumMember(Value = "dagannoth_supreme")]
        DagannothSupreme,

        [EnumMember(Value = "deranged_archaeologist")]
        DerangedArchaeologist,

        [EnumMember(Value = "general_graardor")]
        GeneralGraardor,

        [EnumMember(Value = "giant_mole")]
        GiantMole,

        [EnumMember(Value = "grotesque_guardians")]
        GrotesqueGuardians,

        [EnumMember(Value = "hespori")]
        Hespori,

        [EnumMember(Value = "kalphite_queen")]
        KalphiteQueen,

        [EnumMember(Value = "king_black_dragon")]
        KingBlackDragon,

        [EnumMember(Value = "kraken")]
        Kraken,

        [EnumMember(Value = "kreearra")]
        Kreearra,

        [EnumMember(Value = "kril_tsutsaroth")]
        KrilTsutsaroth,

        [EnumMember(Value = "mimic")]
        Mimic,

        [EnumMember(Value = "nightmare")]
        Nightmare,

        [EnumMember(Value = "obor")]
        Obor,

        [EnumMember(Value = "sarachnis")]
        Sarachnis,

        [EnumMember(Value = "scorpia")]
        Scorpia,

        [EnumMember(Value = "skotizo")]
        Skotizo,

        [EnumMember(Value = "the_gauntlet")]
        TheGauntlet,

        [EnumMember(Value = "the_corrupted_gauntlet")]
        TheCorruptedGauntlet,

        [EnumMember(Value = "theatre_of_blood")]
        TheatreOfBlood,

        [EnumMember(Value = "thermonuclear_smoke_devil")]
        ThermonuclearSmokeDevil,

        [EnumMember(Value = "tzkal_zuk")]
        TzkalZuk,

        [EnumMember(Value = "tztok_jad")]
        TztokJad,

        [EnumMember(Value = "venenatis")]
        Venenatis,

        [EnumMember(Value = "vetion")]
        Vetion,

        [EnumMember(Value = "vorkath")]
        Vorkath,

        [EnumMember(Value = "wintertodt")]
        Wintertodt,

        [EnumMember(Value = "zalcano")]
        Zalcano,

        [EnumMember(Value = "zulrah")]
        Zulrah,

        [EnumMember(Value = "combat")]
        Combat,

        [EnumMember(Value = "ehp")]
        EffectiveHoursPlaying,

        [EnumMember(Value = "ehb")]
        EffectiveHoursBossing,
        
        [EnumMember(Value = "tempoross")]
        Tempoross,
        
        [EnumMember(Value = "phosanis_nightmare")]
        PhosanisNightmare,
        
        [EnumMember(Value = "theatre_of_blood_hard_mode")]
        TheatreOfBloodHardMode
    }
}