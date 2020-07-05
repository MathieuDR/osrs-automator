using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Responses.Models {
    internal class DeltaMetrics {
      public WOMDeltaMetric Overall { get; set; }
        public WOMDeltaMetric Attack { get; set; }
        public WOMDeltaMetric Defence { get; set; }
        public WOMDeltaMetric Strength { get; set; }
        public WOMDeltaMetric Hitpoints { get; set; }
        public WOMDeltaMetric Ranged { get; set; }
        public WOMDeltaMetric Prayer { get; set; }
        public WOMDeltaMetric Magic { get; set; }
        public WOMDeltaMetric Cooking { get; set; }
        public WOMDeltaMetric Woodcutting { get; set; }
        public WOMDeltaMetric Fletching { get; set; }
        public WOMDeltaMetric Fishing { get; set; }
        public WOMDeltaMetric Firemaking { get; set; }
        public WOMDeltaMetric Crafting { get; set; }
        public WOMDeltaMetric Smithing { get; set; }
        public WOMDeltaMetric Mining { get; set; }
        public WOMDeltaMetric Herblore { get; set; }
        public WOMDeltaMetric Agility { get; set; }
        public WOMDeltaMetric Thieving { get; set; }
        public WOMDeltaMetric Slayer { get; set; }
        public WOMDeltaMetric Farming { get; set; }
        public WOMDeltaMetric Runecrafting { get; set; }
        public WOMDeltaMetric Hunter { get; set; }
        public WOMDeltaMetric Construction { get; set; }

        [JsonProperty("league_points")]
        public WOMDeltaMetric LeaguePoints { get; set; }

        [JsonProperty("bounty_hunter_hunter")]
        public WOMDeltaMetric BountyHunterHunter { get; set; }

        [JsonProperty("bounty_hunter_rogue")]
        public WOMDeltaMetric BountyHunterRogue { get; set; }

        [JsonProperty("clue_scrolls_all")]
        public WOMDeltaMetric ClueScrollsAll { get; set; }

        [JsonProperty("clue_scrolls_beginner")]
        public WOMDeltaMetric ClueScrollsBeginner { get; set; }

        [JsonProperty("clue_scrolls_easy")]
        public WOMDeltaMetric ClueScrollsEasy { get; set; }

        [JsonProperty("clue_scrolls_medium")]
        public WOMDeltaMetric ClueScrollsMedium { get; set; }

        [JsonProperty("clue_scrolls_hard")]
        public WOMDeltaMetric ClueScrollsHard { get; set; }

        [JsonProperty("clue_scrolls_elite")]
        public WOMDeltaMetric ClueScrollsElite { get; set; }

        [JsonProperty("clue_scrolls_master")]
        public WOMDeltaMetric ClueScrollsMaster { get; set; }

        [JsonProperty("last_man_standing")]
        public WOMDeltaMetric LastManStanding { get; set; }

        [JsonProperty("abyssal_sire")]
        public WOMDeltaMetric AbyssalSire { get; set; }

        [JsonProperty("alchemical_hydra")]
        public WOMDeltaMetric AlchemicalHydra { get; set; }

        [JsonProperty("barrows_chests")]
        public WOMDeltaMetric BarrowsChests { get; set; }

        [JsonProperty("bryophyta")]
        public WOMDeltaMetric Bryophyta { get; set; }

        [JsonProperty("callisto")]
        public WOMDeltaMetric Callisto { get; set; }

        [JsonProperty("cerberus")]
        public WOMDeltaMetric Cerberus { get; set; }

        [JsonProperty("chambers_of_xeric")]
        public WOMDeltaMetric ChambersOfXeric { get; set; }

        [JsonProperty("chambers_of_xeric_challenge_mode")]
        public WOMDeltaMetric ChambersOfXericChallengeMode { get; set; }

        [JsonProperty("chaos_elemental")]
        public WOMDeltaMetric ChaosElemental { get; set; }

        [JsonProperty("chaos_fanatic")]
        public WOMDeltaMetric ChaosFanatic { get; set; }

        [JsonProperty("commander_zilyana")]
        public WOMDeltaMetric CommanderZilyana { get; set; }

        [JsonProperty("corporeal_beast")]
        public WOMDeltaMetric CorporealBeast { get; set; }

        [JsonProperty("crazy_archaeologist")]
        public WOMDeltaMetric CrazyArchaeologist { get; set; }

        [JsonProperty("dagannoth_prime")]
        public WOMDeltaMetric DagannothPrime { get; set; }

        [JsonProperty("dagannoth_rex")]
        public WOMDeltaMetric DagannothRex { get; set; }

        [JsonProperty("dagannoth_supreme")]
        public WOMDeltaMetric DagannothSupreme { get; set; }

        [JsonProperty("deranged_archaeologist")]
        public WOMDeltaMetric DerangedArchaeologist { get; set; }

        [JsonProperty("general_graardor")]
        public WOMDeltaMetric GeneralGraardor { get; set; }

        [JsonProperty("giant_mole")]
        public WOMDeltaMetric GiantMole { get; set; }

        [JsonProperty("grotesque_guardians")]
        public WOMDeltaMetric GrotesqueGuardians { get; set; }

        [JsonProperty("hespori")]
        public WOMDeltaMetric Hespori { get; set; }

        [JsonProperty("kalphite_queen")]
        public WOMDeltaMetric KalphiteQueen { get; set; }

        [JsonProperty("king_black_dragon")]
        public WOMDeltaMetric KingBlackDragon { get; set; }

        [JsonProperty("kraken")]
        public WOMDeltaMetric Kraken { get; set; }

        [JsonProperty("kreearra")]
        public WOMDeltaMetric Kreearra { get; set; }

        [JsonProperty("kril_tsutsaroth")]
        public WOMDeltaMetric KrilTsutsaroth { get; set; }

        [JsonProperty("mimic")]
        public WOMDeltaMetric Mimic { get; set; }

        [JsonProperty("nightmare")]
        public WOMDeltaMetric Nightmare { get; set; }

        [JsonProperty("obor")]
        public WOMDeltaMetric Obor { get; set; }

        [JsonProperty("sarachnis")]
        public WOMDeltaMetric Sarachnis { get; set; }

        [JsonProperty("scorpia")]
        public WOMDeltaMetric Scorpia { get; set; }

        [JsonProperty("skotizo")]
        public WOMDeltaMetric Skotizo { get; set; }

        [JsonProperty("the_gauntlet")]
        public WOMDeltaMetric TheGauntlet { get; set; }

        [JsonProperty("the_corrupted_gauntlet")]
        public WOMDeltaMetric TheCorruptedGauntlet { get; set; }

        [JsonProperty("theatre_of_blood")]
        public WOMDeltaMetric TheatreOfBlood { get; set; }

        [JsonProperty("thermonuclear_smoke_devil")]
        public WOMDeltaMetric ThermonuclearSmokeDevil { get; set; }

        [JsonProperty("tzkal_zuk")]
        public WOMDeltaMetric TzkalZuk { get; set; }

        [JsonProperty("tztok_jad")]
        public WOMDeltaMetric TztokJad { get; set; }

        [JsonProperty("venenatis")]
        public WOMDeltaMetric Venenatis { get; set; }

        [JsonProperty("vetion")]
        public WOMDeltaMetric Vetion { get; set; }

        [JsonProperty("vorkath")]
        public WOMDeltaMetric Vorkath { get; set; }

        [JsonProperty("wintertodt")]
        public WOMDeltaMetric Wintertodt { get; set; }

        [JsonProperty("zalcano")]
        public WOMDeltaMetric Zalcano { get; set; }

        [JsonProperty("zulrah")]
        public WOMDeltaMetric Zulrah { get; set; }
    }
}