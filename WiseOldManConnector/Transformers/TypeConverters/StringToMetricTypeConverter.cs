using System;
using AutoMapper;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Transformers.TypeConverters; 

internal class StringToMetricTypeConverter : ITypeConverter<string, MetricType> {
    public MetricType Convert(string source, MetricType destination, ResolutionContext context) {
        //var lowerInvariant = source.ToLowerInvariant();
        if (Enum.TryParse(typeof(MetricType), source, true, out var temp)) {
            destination = (MetricType) temp;
            return destination;
        }


        if (string.IsNullOrWhiteSpace(source)) {
            throw new Exception($"{source} is not a metric type.");
        }

        // Do we want to keep this, or use enum members / parsing etc?
        var lowerInvariant = source.ToLowerInvariant();
        destination = lowerInvariant switch {
            "overall" => MetricType.Overall,
            "attack" => MetricType.Attack,
            "defence" => MetricType.Defence,
            "strength" => MetricType.Strength,
            "hitpoints" => MetricType.Hitpoints,
            "ranged" => MetricType.Ranged,
            "prayer" => MetricType.Prayer,
            "magic" => MetricType.Magic,
            "cooking" => MetricType.Cooking,
            "woodcutting" => MetricType.Woodcutting,
            "fletching" => MetricType.Fletching,
            "fishing" => MetricType.Fishing,
            "firemaking" => MetricType.Firemaking,
            "crafting" => MetricType.Crafting,
            "smithing" => MetricType.Smithing,
            "mining" => MetricType.Mining,
            "herblore" => MetricType.Herblore,
            "agility" => MetricType.Agility,
            "thieving" => MetricType.Thieving,
            "slayer" => MetricType.Slayer,
            "farming" => MetricType.Farming,
            "runecrafting" => MetricType.Runecrafting,
            "hunter" => MetricType.Hunter,
            "Construction" => MetricType.Construction,
            "league_points" => MetricType.LeaguePoints,
            "bounty_hunter_hunter" => MetricType.BountyHunterHunter,
            "bounty_hunter_rogue" => MetricType.BountyHunterRogue,
            "clue_scrolls_all" => MetricType.ClueScrollsAll,
            "clue_scrolls_beginner" => MetricType.ClueScrollsBeginner,
            "clue_scrolls_easy" => MetricType.ClueScrollsEasy,
            "clue_scrolls_medium" => MetricType.ClueScrollsMedium,
            "clue_scrolls_hard" => MetricType.ClueScrollsHard,
            "clue_scrolls_elite" => MetricType.ClueScrollsElite,
            "clue_scrolls_master" => MetricType.ClueScrollsMaster,
            "last_man_standing" => MetricType.LastManStanding,
            "soul_wars_zeal" => MetricType.SoulWarsZeal,
            "abyssal_sire" => MetricType.AbyssalSire,
            "alchemical_hydra" => MetricType.AlchemicalHydra,
            "barrows_chests" => MetricType.BarrowsChests,
            "bryophyta" => MetricType.Bryophyta,
            "callisto" => MetricType.Callisto,
            "cerberus" => MetricType.Cerberus,
            "chambers_of_xeric" => MetricType.ChambersOfXeric,
            "chambers_of_xeric_challenge_mode" => MetricType.ChambersOfXericChallengeMode,
            "chaos_elemental" => MetricType.ChaosElemental,
            "chaos_fanatic" => MetricType.ChaosFanatic,
            "commander_zilyana" => MetricType.CommanderZilyana,
            "corporeal_beast" => MetricType.CorporealBeast,
            "crazy_archaeologist" => MetricType.CrazyArchaeologist,
            "dagannoth_prime" => MetricType.DagannothPrime,
            "dagannoth_rex" => MetricType.DagannothRex,
            "dagannoth_supreme" => MetricType.DagannothSupreme,
            "deranged_archaeologist" => MetricType.DerangedArchaeologist,
            "general_graardor" => MetricType.GeneralGraardor,
            "giant_mole" => MetricType.GiantMole,
            "grotesque_guardians" => MetricType.GrotesqueGuardians,
            "hespori" => MetricType.Hespori,
            "kalphite_queen" => MetricType.KalphiteQueen,
            "king_black_dragon" => MetricType.KingBlackDragon,
            "kraken" => MetricType.Kraken,
            "kreearra" => MetricType.Kreearra,
            "kril_tsutsaroth" => MetricType.KrilTsutsaroth,
            "mimic" => MetricType.Mimic,
            "nightmare" => MetricType.Nightmare,
            "phosanis_nightmare" => MetricType.PhosanisNightmare,
            "obor" => MetricType.Obor,
            "sarachnis" => MetricType.Sarachnis,
            "scorpia" => MetricType.Scorpia,
            "skotizo" => MetricType.Skotizo,
            "the_gauntlet" => MetricType.TheGauntlet,
            "the_corrupted_gauntlet" => MetricType.TheCorruptedGauntlet,
            "theatre_of_blood" => MetricType.TheatreOfBlood,
            "theatre_of_blood_hard_mode" => MetricType.TheatreOfBloodHardMode,
            "thermonuclear_smoke_devil" => MetricType.ThermonuclearSmokeDevil,
            "tzkal_zuk" => MetricType.TzkalZuk,
            "tztok_jad" => MetricType.TztokJad,
            "venenatis" => MetricType.Venenatis,
            "vetion" => MetricType.Vetion,
            "vorkath" => MetricType.Vorkath,
            "tempoross" => MetricType.Tempoross,
            "wintertodt" => MetricType.Wintertodt,
            "zalcano" => MetricType.Zalcano,
            "zulrah" => MetricType.Zulrah,
            "combat" => MetricType.Combat,
            "ehb" => MetricType.EffectiveHoursBossing,
            "ehp" => MetricType.EffectiveHoursPlaying,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };

        return destination;
    }
}