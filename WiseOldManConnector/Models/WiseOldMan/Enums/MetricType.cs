using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace WiseOldManConnector.Models.WiseOldMan.Enums;

public enum MetricType {
    [EnumMember(Value = "overall")]
    [Display(Name = "Overall")]
    Overall = 1,

    [EnumMember(Value = "attack")]
    [Display(Name = "Attack")]
    Attack,

    [EnumMember(Value = "defence")]
    [Display(Name = "Defence")]
    Defence,

    [EnumMember(Value = "strength")]
    [Display(Name = "Strength")]    
    Strength,

    [EnumMember(Value = "hitpoints")]
    [Display(Name = "Hitpoints")]
    Hitpoints,

    [EnumMember(Value = "ranged")]
    [Display(Name = "Ranged")]
    Ranged,

    [EnumMember(Value = "prayer")]
    [Display(Name = "Prayer")]
    Prayer,

    [EnumMember(Value = "magic")]
    [Display(Name = "Magic")]
    Magic,

    [EnumMember(Value = "cooking")]
    [Display(Name = "Cooking")]
    Cooking,

    [EnumMember(Value = "woodcutting")]
    [Display(Name = "Woodcutting")]
    Woodcutting,

    [EnumMember(Value = "fletching")]
    [Display(Name = "Fletching")]
    Fletching,

    [EnumMember(Value = "fishing")]
    [Display(Name = "Fishing")]
    Fishing,

    [EnumMember(Value = "firemaking")]
    [Display(Name = "Firemaking")]
    Firemaking,

    [EnumMember(Value = "crafting")]
    [Display(Name = "Crafting")]
    Crafting,

    [EnumMember(Value = "smithing")]
    [Display(Name = "Smithing")]
    Smithing,

    [EnumMember(Value = "mining")]
    [Display(Name = "Mining")]
    Mining,

    [EnumMember(Value = "herblore")]
    [Display(Name = "Herblore")]
    Herblore,

    [EnumMember(Value = "agility")]
    [Display(Name = "Agility")]
    Agility,

    [EnumMember(Value = "thieving")]
    [Display(Name = "Thieving")]
    Thieving,

    [EnumMember(Value = "slayer")]
    [Display(Name = "Slayer")]
    Slayer,

    [EnumMember(Value = "farming")]
    [Display(Name = "Farming")]
    Farming,

    [EnumMember(Value = "runecrafting")]
    [Display(Name = "Runecrafting")]
    Runecrafting,

    [EnumMember(Value = "hunter")]
    [Display(Name = "Hunter")]
    Hunter,

    [EnumMember(Value = "construction")]
    [Display(Name = "Construction")]
    Construction,

    [EnumMember(Value = "league_points")]
    [Display(Name = "League Points")]
    LeaguePoints = 50,

    [EnumMember(Value = "bounty_hunter_hunter")]
    [Display(Name = "Bounty Hunter Hunter")]
    BountyHunterHunter,

    [EnumMember(Value = "bounty_hunter_rogue")]
    [Display(Name = "Bounty Hunter Rogue")]
    BountyHunterRogue,

    [EnumMember(Value = "clue_scrolls_all")]
    [Display(Name = "Clue Scrolls")]
    ClueScrollsAll,

    [EnumMember(Value = "clue_scrolls_beginner")]
    [Display(Name = "Beginner Clue Scrolls")]
    ClueScrollsBeginner,

    [EnumMember(Value = "clue_scrolls_easy")]
    [Display(Name = "Easy Clue Scrolls")]
    ClueScrollsEasy,

    [EnumMember(Value = "clue_scrolls_medium")]
    [Display(Name = "Medium Clue Scrolls")]
    ClueScrollsMedium,

    [EnumMember(Value = "clue_scrolls_hard")]
    [Display(Name = "Hard Clue Scrolls")]
    ClueScrollsHard,

    [EnumMember(Value = "clue_scrolls_elite")]
    [Display(Name = "Elite Clue Scrolls")]
    ClueScrollsElite,

    [EnumMember(Value = "clue_scrolls_master")]
    [Display(Name = "Master Clue Scrolls")]
    ClueScrollsMaster,

    [EnumMember(Value = "last_man_standing")]
    [Display(Name = "Last man standing")]
    LastManStanding,

    [EnumMember(Value = "soul_wars_zeal")]
    [Display(Name = "Soul wars")]
    SoulWarsZeal,

    [EnumMember(Value = "abyssal_sire")]
    [Display(Name = "Abyssal Sire")]
    AbyssalSire,

    [EnumMember(Value = "alchemical_hydra")]
    [Display(Name = "Alchemical Hydra")]
    AlchemicalHydra,

    [EnumMember(Value = "barrows_chests")]
    [Display(Name = "Barrows")]
    BarrowsChests,

    [EnumMember(Value = "bryophyta")]
    [Display(Name = "Bryophyta")]
    Bryophyta,

    [EnumMember(Value = "callisto")]
    [Display(Name = "Callisto")]
    Callisto,

    [EnumMember(Value = "cerberus")]
    [Display(Name = "Cerberus")]
    Cerberus,

    [EnumMember(Value = "chambers_of_xeric")]
    [Display(Name = "Chambers of Xeric")]
    ChambersOfXeric,

    [EnumMember(Value = "chambers_of_xeric_challenge_mode")]
    [Display(Name = "Chambers of Xeric Challenge Mode")]
    ChambersOfXericChallengeMode,

    [EnumMember(Value = "chaos_elemental")]
    [Display(Name = "Chaos Elemental")]
    ChaosElemental,

    [EnumMember(Value = "chaos_fanatic")]
    [Display(Name = "Chaos Fanatic")]
    ChaosFanatic,

    [EnumMember(Value = "commander_zilyana")]
    [Display(Name = "Commander Zilyana")]
    CommanderZilyana,

    [EnumMember(Value = "corporeal_beast")]
    [Display(Name = "Corporeal Beast")]
    CorporealBeast,

    [EnumMember(Value = "crazy_archaeologist")]
    [Display(Name = "Crazy Archaeologist")]
    CrazyArchaeologist,

    [EnumMember(Value = "dagannoth_prime")]
    [Display(Name = "Dagannoth Prime")]
    DagannothPrime,

    [EnumMember(Value = "dagannoth_rex")]
    [Display(Name = "Dagannoth Rex")]
    DagannothRex,

    [EnumMember(Value = "dagannoth_supreme")]
        [Display(Name = "Dagannoth Supreme")]
    DagannothSupreme,

    [EnumMember(Value = "deranged_archaeologist")]
    [Display(Name = "Deranged Archaeologist")]
    DerangedArchaeologist,

    [EnumMember(Value = "general_graardor")]
    [Display(Name = "General Graardor")]
    GeneralGraardor,

    [EnumMember(Value = "giant_mole")]
    [Display(Name = "Giant Mole")]
    GiantMole,

    [EnumMember(Value = "grotesque_guardians")]
    [Display(Name = "Grotesque Guardians")]
    GrotesqueGuardians,

    [EnumMember(Value = "hespori")]
    [Display(Name = "Hespori")]
    Hespori,

    [EnumMember(Value = "kalphite_queen")]
    [Display(Name = "Kalphite Queen")]
    KalphiteQueen,

    [EnumMember(Value = "king_black_dragon")]
    [Display(Name = "King Black Dragon")]
    KingBlackDragon,

    [EnumMember(Value = "kraken")]
    [Display(Name = "Kraken")]
    Kraken,

    [EnumMember(Value = "kreearra")]
    [Display(Name = "Kreearra")]
    Kreearra,

    [EnumMember(Value = "kril_tsutsaroth")]
    [Display(Name = "Kril Tsutsaroth")]
    KrilTsutsaroth,

    [EnumMember(Value = "mimic")]
    [Display(Name = "Mimic")]
    Mimic,

    [EnumMember(Value = "nightmare")]
    [Display(Name = "Nightmare")]
    Nightmare,

    [EnumMember(Value = "obor")]
    [Display(Name = "Obor")]
    Obor,

    [EnumMember(Value = "sarachnis")]
    [Display(Name = "Sarachnis")]
    Sarachnis,

    [EnumMember(Value = "scorpia")]
    [Display(Name = "Scorpia")]
    Scorpia,

    [EnumMember(Value = "skotizo")]
    [Display(Name = "Skotizo")]
    Skotizo,

    [EnumMember(Value = "the_gauntlet")]
    [Display(Name = "The Gauntlet")]
    TheGauntlet,

    [EnumMember(Value = "the_corrupted_gauntlet")]
    [Display(Name = "The Corrupted Gauntlet")]
    TheCorruptedGauntlet,

    [EnumMember(Value = "theatre_of_blood")]
    [Display(Name = "Theatre of Blood")]
    TheatreOfBlood,

    [EnumMember(Value = "thermonuclear_smoke_devil")]
    [Display(Name = "Thermonuclear Smoke Devil")]
    ThermonuclearSmokeDevil,

    [EnumMember(Value = "tzkal_zuk")]
    [Display(Name = "TzKal-Zuk")]
    TzkalZuk,

    [EnumMember(Value = "tztok_jad")]
    [Display(Name = "TzTok-Jad")]
    TztokJad,

    [EnumMember(Value = "venenatis")]
    [Display(Name = "Venenatis")]
    Venenatis,


    [EnumMember(Value = "vetion")]
    [Display(Name = "Vet'ion")]
    Vetion,

    [EnumMember(Value = "vorkath")]
    [Display(Name = "Vorkath")]
    Vorkath,

    [EnumMember(Value = "wintertodt")]
    [Display(Name = "Wintertodt")]
    Wintertodt,

    [EnumMember(Value = "zalcano")]
    [Display(Name = "Zalcano")]
    Zalcano,

    [EnumMember(Value = "zulrah")]
    [Display(Name = "Zulrah")]
    Zulrah,

    [EnumMember(Value = "combat")]
    [Display(Name = "Combat")]
    Combat,

    [EnumMember(Value = "ehp")]
    [Display(Name = "EHP")]
    EffectiveHoursPlaying,

    [EnumMember(Value = "ehb")]
    [Display(Name = "EHB")]
    EffectiveHoursBossing,

    [EnumMember(Value = "tempoross")]
    [Display(Name = "Tempoross")]
    Tempoross,

    [EnumMember(Value = "phosanis_nightmare")]
    [Display(Name = "Phosanis Nightmare")]
    PhosanisNightmare,

    [EnumMember(Value = "theatre_of_blood_hard_mode")]
    [Display(Name = "Theatre of Blood Hard Mode")]
    TheatreOfBloodHardMode
}
