using System;
using Newtonsoft.Json;

namespace WiseOldManConnector.Models.API.Responses.Models {
    internal class Snapshot {
        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("importedAt")]
        public object ImportedAt { get; set; }

        [JsonProperty("overall")]
        public Metric Overall { get; set; }

        [JsonProperty("attack")]
        public Metric Attack { get; set; }

        [JsonProperty("defence")]
        public Metric Defence { get; set; }

        [JsonProperty("strength")]
        public Metric Strength { get; set; }

        [JsonProperty("hitpoints")]
        public Metric Hitpoints { get; set; }

        [JsonProperty("ranged")]
        public Metric Ranged { get; set; }

        [JsonProperty("prayer")]
        public Metric Prayer { get; set; }

        [JsonProperty("magic")]
        public Metric Magic { get; set; }

        [JsonProperty("cooking")]
        public Metric Cooking { get; set; }

        [JsonProperty("woodcutting")]
        public Metric Woodcutting { get; set; }

        [JsonProperty("fletching")]
        public Metric Fletching { get; set; }

        [JsonProperty("fishing")]
        public Metric Fishing { get; set; }

        [JsonProperty("firemaking")]
        public Metric Firemaking { get; set; }

        [JsonProperty("crafting")]
        public Metric Crafting { get; set; }

        [JsonProperty("smithing")]
        public Metric Smithing { get; set; }

        [JsonProperty("mining")]
        public Metric Mining { get; set; }

        [JsonProperty("herblore")]
        public Metric Herblore { get; set; }

        [JsonProperty("agility")]
        public Metric Agility { get; set; }

        [JsonProperty("thieving")]
        public Metric Thieving { get; set; }

        [JsonProperty("slayer")]
        public Metric Slayer { get; set; }

        [JsonProperty("farming")]
        public Metric Farming { get; set; }

        [JsonProperty("runecrafting")]
        public Metric Runecrafting { get; set; }

        [JsonProperty("hunter")]
        public Metric Hunter { get; set; }

        [JsonProperty("construction")]
        public Metric Construction { get; set; }

        [JsonProperty("league_points")]
        public Metric LeaguePoints { get; set; }

        [JsonProperty("bounty_hunter_hunter")]
        public Metric BountyHunterHunter { get; set; }

        [JsonProperty("bounty_hunter_rogue")]
        public Metric BountyHunterRogue { get; set; }

        [JsonProperty("clue_scrolls_all")]
        public Metric ClueScrollsAll { get; set; }

        [JsonProperty("clue_scrolls_beginner")]
        public Metric ClueScrollsBeginner { get; set; }

        [JsonProperty("clue_scrolls_easy")]
        public Metric ClueScrollsEasy { get; set; }

        [JsonProperty("clue_scrolls_medium")]
        public Metric ClueScrollsMedium { get; set; }

        [JsonProperty("clue_scrolls_hard")]
        public Metric ClueScrollsHard { get; set; }

        [JsonProperty("clue_scrolls_elite")]
        public Metric ClueScrollsElite { get; set; }

        [JsonProperty("clue_scrolls_master")]
        public Metric ClueScrollsMaster { get; set; }

        [JsonProperty("last_man_standing")]
        public Metric LastManStanding { get; set; }

        [JsonProperty("abyssal_sire")]
        public Metric AbyssalSire { get; set; }

        [JsonProperty("alchemical_hydra")]
        public Metric AlchemicalHydra { get; set; }

        [JsonProperty("barrows_chests")]
        public Metric BarrowsChests { get; set; }

        [JsonProperty("bryophyta")]
        public Metric Bryophyta { get; set; }

        [JsonProperty("callisto")]
        public Metric Callisto { get; set; }

        [JsonProperty("cerberus")]
        public Metric Cerberus { get; set; }

        [JsonProperty("chambers_of_xeric")]
        public Metric ChambersOfXeric { get; set; }

        [JsonProperty("chambers_of_xeric_challenge_mode")]
        public Metric ChambersOfXericChallengeMode { get; set; }

        [JsonProperty("chaos_elemental")]
        public Metric ChaosElemental { get; set; }

        [JsonProperty("chaos_fanatic")]
        public Metric ChaosFanatic { get; set; }

        [JsonProperty("commander_zilyana")]
        public Metric CommanderZilyana { get; set; }

        [JsonProperty("corporeal_beast")]
        public Metric CorporealBeast { get; set; }

        [JsonProperty("crazy_archaeologist")]
        public Metric CrazyArchaeologist { get; set; }

        [JsonProperty("dagannoth_prime")]
        public Metric DagannothPrime { get; set; }

        [JsonProperty("dagannoth_rex")]
        public Metric DagannothRex { get; set; }

        [JsonProperty("dagannoth_supreme")]
        public Metric DagannothSupreme { get; set; }

        [JsonProperty("deranged_archaeologist")]
        public Metric DerangedArchaeologist { get; set; }

        [JsonProperty("general_graardor")]
        public Metric GeneralGraardor { get; set; }

        [JsonProperty("giant_mole")]
        public Metric GiantMole { get; set; }

        [JsonProperty("grotesque_guardians")]
        public Metric GrotesqueGuardians { get; set; }

        [JsonProperty("hespori")]
        public Metric Hespori { get; set; }

        [JsonProperty("kalphite_queen")]
        public Metric KalphiteQueen { get; set; }

        [JsonProperty("king_black_dragon")]
        public Metric KingBlackDragon { get; set; }

        [JsonProperty("kraken")]
        public Metric Kraken { get; set; }

        [JsonProperty("kreearra")]
        public Metric Kreearra { get; set; }

        [JsonProperty("kril_tsutsaroth")]
        public Metric KrilTsutsaroth { get; set; }

        [JsonProperty("mimic")]
        public Metric Mimic { get; set; }

        [JsonProperty("nightmare")]
        public Metric Nightmare { get; set; }

        [JsonProperty("obor")]
        public Metric Obor { get; set; }

        [JsonProperty("sarachnis")]
        public Metric Sarachnis { get; set; }

        [JsonProperty("scorpia")]
        public Metric Scorpia { get; set; }

        [JsonProperty("skotizo")]
        public Metric Skotizo { get; set; }

        [JsonProperty("the_gauntlet")]
        public Metric TheGauntlet { get; set; }

        [JsonProperty("the_corrupted_gauntlet")]
        public Metric TheCorruptedGauntlet { get; set; }

        [JsonProperty("theatre_of_blood")]
        public Metric TheatreOfBlood { get; set; }

        [JsonProperty("thermonuclear_smoke_devil")]
        public Metric ThermonuclearSmokeDevil { get; set; }

        [JsonProperty("tzkal_zuk")]
        public Metric TzkalZuk { get; set; }

        [JsonProperty("tztok_jad")]
        public Metric TztokJad { get; set; }

        [JsonProperty("venenatis")]
        public Metric Venenatis { get; set; }

        [JsonProperty("vetion")]
        public Metric Vetion { get; set; }

        [JsonProperty("vorkath")]
        public Metric Vorkath { get; set; }

        [JsonProperty("wintertodt")]
        public Metric Wintertodt { get; set; }

        [JsonProperty("zalcano")]
        public Metric Zalcano { get; set; }

        [JsonProperty("zulrah")]
        public Metric Zulrah { get; set; }
    }
}