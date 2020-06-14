using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Models.API.Responses.Models {
    public class DeltaMetrics {
        private Dictionary<MetricType, DeltaMetric> _allDeltaMetrics = null;
        private Dictionary<MetricType, DeltaMetric> _bossDeltaMetrics = null;
        private Dictionary<MetricType, DeltaMetric> _skillDeltaMetrics = null;
        private List<DeltaInfo> _allDeltaMetricsInfos = null;
        private List<DeltaInfo> _bossDeltaMetricsInfos = null;
        private List<DeltaInfo> _skillDeltaMetricsInfos = null;
        public DeltaMetric Overall { get; set; }
        public DeltaMetric Attack { get; set; }
        public DeltaMetric Defence { get; set; }
        public DeltaMetric Strength { get; set; }
        public DeltaMetric Hitpoints { get; set; }
        public DeltaMetric Ranged { get; set; }
        public DeltaMetric Prayer { get; set; }
        public DeltaMetric Magic { get; set; }
        public DeltaMetric Cooking { get; set; }
        public DeltaMetric Woodcutting { get; set; }
        public DeltaMetric Fletching { get; set; }
        public DeltaMetric Fishing { get; set; }
        public DeltaMetric Firemaking { get; set; }
        public DeltaMetric Crafting { get; set; }
        public DeltaMetric Smithing { get; set; }
        public DeltaMetric Mining { get; set; }
        public DeltaMetric Herblore { get; set; }
        public DeltaMetric Agility { get; set; }
        public DeltaMetric Thieving { get; set; }
        public DeltaMetric Slayer { get; set; }
        public DeltaMetric Farming { get; set; }
        public DeltaMetric Runecrafting { get; set; }
        public DeltaMetric Hunter { get; set; }
        public DeltaMetric Construction { get; set; }

        [JsonProperty("league_points")]
        public DeltaMetric LeaguePoints { get; set; }

        [JsonProperty("bounty_hunter_hunter")]
        public DeltaMetric BountyHunterHunter { get; set; }

        [JsonProperty("bounty_hunter_rogue")]
        public DeltaMetric BountyHunterRogue { get; set; }

        [JsonProperty("clue_scrolls_all")]
        public DeltaMetric ClueScrollsAll { get; set; }

        [JsonProperty("clue_scrolls_beginner")]
        public DeltaMetric ClueScrollsBeginner { get; set; }

        [JsonProperty("clue_scrolls_easy")]
        public DeltaMetric ClueScrollsEasy { get; set; }

        [JsonProperty("clue_scrolls_medium")]
        public DeltaMetric ClueScrollsMedium { get; set; }

        [JsonProperty("clue_scrolls_hard")]
        public DeltaMetric ClueScrollsHard { get; set; }

        [JsonProperty("clue_scrolls_elite")]
        public DeltaMetric ClueScrollsElite { get; set; }

        [JsonProperty("clue_scrolls_master")]
        public DeltaMetric ClueScrollsMaster { get; set; }

        [JsonProperty("last_man_standing")]
        public DeltaMetric LastManStanding { get; set; }

        [JsonProperty("abyssal_sire")]
        public DeltaMetric AbyssalSire { get; set; }

        [JsonProperty("alchemical_hydra")]
        public DeltaMetric AlchemicalHydra { get; set; }

        [JsonProperty("barrows_chests")]
        public DeltaMetric BarrowsChests { get; set; }

        [JsonProperty("bryophyta")]
        public DeltaMetric Bryophyta { get; set; }

        [JsonProperty("callisto")]
        public DeltaMetric Callisto { get; set; }

        [JsonProperty("cerberus")]
        public DeltaMetric Cerberus { get; set; }

        [JsonProperty("chambers_of_xeric")]
        public DeltaMetric ChambersOfXeric { get; set; }

        [JsonProperty("chambers_of_xeric_challenge_mode")]
        public DeltaMetric ChambersOfXericChallengeMode { get; set; }

        [JsonProperty("chaos_elemental")]
        public DeltaMetric ChaosElemental { get; set; }

        [JsonProperty("chaos_fanatic")]
        public DeltaMetric ChaosFanatic { get; set; }

        [JsonProperty("commander_zilyana")]
        public DeltaMetric CommanderZilyana { get; set; }

        [JsonProperty("corporeal_beast")]
        public DeltaMetric CorporealBeast { get; set; }

        [JsonProperty("crazy_archaeologist")]
        public DeltaMetric CrazyArchaeologist { get; set; }

        [JsonProperty("dagannoth_prime")]
        public DeltaMetric DagannothPrime { get; set; }

        [JsonProperty("dagannoth_rex")]
        public DeltaMetric DagannothRex { get; set; }

        [JsonProperty("dagannoth_supreme")]
        public DeltaMetric DagannothSupreme { get; set; }

        [JsonProperty("deranged_archaeologist")]
        public DeltaMetric DerangedArchaeologist { get; set; }

        [JsonProperty("general_graardor")]
        public DeltaMetric GeneralGraardor { get; set; }

        [JsonProperty("giant_mole")]
        public DeltaMetric GiantMole { get; set; }

        [JsonProperty("grotesque_guardians")]
        public DeltaMetric GrotesqueGuardians { get; set; }

        [JsonProperty("hespori")]
        public DeltaMetric Hespori { get; set; }

        [JsonProperty("kalphite_queen")]
        public DeltaMetric KalphiteQueen { get; set; }

        [JsonProperty("king_black_dragon")]
        public DeltaMetric KingBlackDragon { get; set; }

        [JsonProperty("kraken")]
        public DeltaMetric Kraken { get; set; }

        [JsonProperty("kreearra")]
        public DeltaMetric Kreearra { get; set; }

        [JsonProperty("kril_tsutsaroth")]
        public DeltaMetric KrilTsutsaroth { get; set; }

        [JsonProperty("mimic")]
        public DeltaMetric Mimic { get; set; }

        [JsonProperty("nightmare")]
        public DeltaMetric Nightmare { get; set; }

        [JsonProperty("obor")]
        public DeltaMetric Obor { get; set; }

        [JsonProperty("sarachnis")]
        public DeltaMetric Sarachnis { get; set; }

        [JsonProperty("scorpia")]
        public DeltaMetric Scorpia { get; set; }

        [JsonProperty("skotizo")]
        public DeltaMetric Skotizo { get; set; }

        [JsonProperty("the_gauntlet")]
        public DeltaMetric TheGauntlet { get; set; }

        [JsonProperty("the_corrupted_gauntlet")]
        public DeltaMetric TheCorruptedGauntlet { get; set; }

        [JsonProperty("theatre_of_blood")]
        public DeltaMetric TheatreOfBlood { get; set; }

        [JsonProperty("thermonuclear_smoke_devil")]
        public DeltaMetric ThermonuclearSmokeDevil { get; set; }

        [JsonProperty("tzkal_zuk")]
        public DeltaMetric TzkalZuk { get; set; }

        [JsonProperty("tztok_jad")]
        public DeltaMetric TztokJad { get; set; }

        [JsonProperty("venenatis")]
        public DeltaMetric Venenatis { get; set; }

        [JsonProperty("vetion")]
        public DeltaMetric Vetion { get; set; }

        [JsonProperty("vorkath")]
        public DeltaMetric Vorkath { get; set; }

        [JsonProperty("wintertodt")]
        public DeltaMetric Wintertodt { get; set; }

        [JsonProperty("zalcano")]
        public DeltaMetric Zalcano { get; set; }

        [JsonProperty("zulrah")]
        public DeltaMetric Zulrah { get; set; }

        public Dictionary<MetricType, DeltaMetric> AllDeltaMetrics {
            get {
                return _allDeltaMetrics ??= new Dictionary<MetricType, DeltaMetric> {
                    {MetricType.Overall, Overall},
                    {MetricType.Attack, Attack},
                    {MetricType.Defence, Defence},
                    {MetricType.Strength, Strength},
                    {MetricType.Hitpoints, Hitpoints},
                    {MetricType.Ranged, Ranged},
                    {MetricType.Prayer, Prayer},
                    {MetricType.Magic, Magic},
                    {MetricType.Cooking, Cooking},
                    {MetricType.Woodcutting, Woodcutting},
                    {MetricType.Fletching, Fletching},
                    {MetricType.Fishing, Fishing},
                    {MetricType.Firemaking, Firemaking},
                    {MetricType.Crafting, Crafting},
                    {MetricType.Smithing, Smithing},
                    {MetricType.Mining, Mining},
                    {MetricType.Herblore, Herblore},
                    {MetricType.Agility, Agility},
                    {MetricType.Thieving, Thieving},
                    {MetricType.Slayer, Slayer},
                    {MetricType.Farming, Farming},
                    {MetricType.Runecrafting, Runecrafting},
                    {MetricType.Hunter, Hunter},
                    {MetricType.Construction, Construction},
                    {MetricType.LeaguePoints, LeaguePoints},
                    {MetricType.BountyHunterHunter, BountyHunterHunter},
                    {MetricType.BountyHunterRogue, BountyHunterRogue},
                    {MetricType.ClueScrollsAll, ClueScrollsAll},
                    {MetricType.ClueScrollsEasy, ClueScrollsEasy},
                    {MetricType.ClueScrollsMedium, ClueScrollsMedium},
                    {MetricType.ClueScrollsHard, ClueScrollsHard},
                    {MetricType.ClueScrollsElite, ClueScrollsElite},
                    {MetricType.ClueScrollsMaster, ClueScrollsMaster},
                    {MetricType.LastManStanding, LastManStanding},
                    {MetricType.AbyssalSire, AbyssalSire},
                    {MetricType.AlchemicalHydra, AlchemicalHydra},
                    {MetricType.BarrowsChests, BarrowsChests},
                    {MetricType.Bryophyta, Bryophyta},
                    {MetricType.Callisto, Callisto},
                    {MetricType.Cerberus, Cerberus},
                    {MetricType.ChambersOfXeric, ChambersOfXeric},
                    {MetricType.ChambersOfXericChallengeMode, ChambersOfXericChallengeMode},
                    {MetricType.ChaosElemental, ChaosElemental},
                    {MetricType.ChaosFanatic, ChaosFanatic},
                    {MetricType.CommanderZilyana, CommanderZilyana},
                    {MetricType.CorporealBeast, CorporealBeast},
                    {MetricType.CrazyArchaeologist, CrazyArchaeologist},
                    {MetricType.DagannothPrime, DagannothPrime},
                    {MetricType.DagannothRex, DagannothRex},
                    {MetricType.DagannothSupreme, DagannothSupreme},
                    {MetricType.DerangedArchaeologist, DerangedArchaeologist},
                    {MetricType.GeneralGraardor, GeneralGraardor},
                    {MetricType.GiantMole, GiantMole},
                    {MetricType.GrotesqueGuardians, GrotesqueGuardians},
                    {MetricType.Hespori, Hespori},
                    {MetricType.KalphiteQueen, KalphiteQueen},
                    {MetricType.KingBlackDragon, KingBlackDragon},
                    {MetricType.Kraken, Kraken},
                    {MetricType.Kreearra, Kreearra},
                    {MetricType.KrilTsutsaroth, KrilTsutsaroth},
                    {MetricType.Mimic, Mimic},
                    {MetricType.Nightmare, Nightmare},
                    {MetricType.Obor, Obor},
                    {MetricType.Sarachnis, Sarachnis},
                    {MetricType.Scorpia, Scorpia},
                    {MetricType.Skotizo, Skotizo},
                    {MetricType.TheGauntlet, TheGauntlet},
                    {MetricType.TheCorruptedGauntlet, TheCorruptedGauntlet},
                    {MetricType.TheatreOfBlood, TheatreOfBlood},
                    {MetricType.ThermonuclearSmokeDevil, ThermonuclearSmokeDevil},
                    {MetricType.TzkalZuk, TzkalZuk},
                    {MetricType.TztokJad, TztokJad},
                    {MetricType.Venenatis, Venenatis},
                    {MetricType.Vetion, Vetion},
                    {MetricType.Vorkath, Vorkath},
                    {MetricType.Wintertodt, Wintertodt},
                    {MetricType.Zalcano, Zalcano},
                    {MetricType.Zulrah, Zulrah}
                };
            }
        }

        public Dictionary<MetricType, DeltaMetric> SkillDeltaMetrics {
            get {
                return _skillDeltaMetrics ??= new Dictionary<MetricType, DeltaMetric> {
                    {MetricType.Overall, Overall},
                    {MetricType.Attack, Attack},
                    {MetricType.Defence, Defence},
                    {MetricType.Strength, Strength},
                    {MetricType.Hitpoints, Hitpoints},
                    {MetricType.Ranged, Ranged},
                    {MetricType.Prayer, Prayer},
                    {MetricType.Magic, Magic},
                    {MetricType.Cooking, Cooking},
                    {MetricType.Woodcutting, Woodcutting},
                    {MetricType.Fletching, Fletching},
                    {MetricType.Fishing, Fishing},
                    {MetricType.Firemaking, Firemaking},
                    {MetricType.Crafting, Crafting},
                    {MetricType.Smithing, Smithing},
                    {MetricType.Mining, Mining},
                    {MetricType.Herblore, Herblore},
                    {MetricType.Agility, Agility},
                    {MetricType.Thieving, Thieving},
                    {MetricType.Slayer, Slayer},
                    {MetricType.Farming, Farming},
                    {MetricType.Runecrafting, Runecrafting},
                    {MetricType.Hunter, Hunter},
                    {MetricType.Construction, Construction}
                };
            }
        }

        public Dictionary<MetricType, DeltaMetric> BossDeltaMetrics {
            get {
                return _bossDeltaMetrics ??= new Dictionary<MetricType, DeltaMetric> {
                    {MetricType.LeaguePoints, LeaguePoints},
                    {MetricType.BountyHunterHunter, BountyHunterHunter},
                    {MetricType.BountyHunterRogue, BountyHunterRogue},
                    {MetricType.ClueScrollsAll, ClueScrollsAll},
                    {MetricType.ClueScrollsEasy, ClueScrollsEasy},
                    {MetricType.ClueScrollsMedium, ClueScrollsMedium},
                    {MetricType.ClueScrollsHard, ClueScrollsHard},
                    {MetricType.ClueScrollsElite, ClueScrollsElite},
                    {MetricType.ClueScrollsMaster, ClueScrollsMaster},
                    {MetricType.LastManStanding, LastManStanding},
                    {MetricType.AbyssalSire, AbyssalSire},
                    {MetricType.AlchemicalHydra, AlchemicalHydra},
                    {MetricType.BarrowsChests, BarrowsChests},
                    {MetricType.Bryophyta, Bryophyta},
                    {MetricType.Callisto, Callisto},
                    {MetricType.Cerberus, Cerberus},
                    {MetricType.ChambersOfXeric, ChambersOfXeric},
                    {MetricType.ChambersOfXericChallengeMode, ChambersOfXericChallengeMode},
                    {MetricType.ChaosElemental, ChaosElemental},
                    {MetricType.ChaosFanatic, ChaosFanatic},
                    {MetricType.CommanderZilyana, CommanderZilyana},
                    {MetricType.CorporealBeast, CorporealBeast},
                    {MetricType.CrazyArchaeologist, CrazyArchaeologist},
                    {MetricType.DagannothPrime, DagannothPrime},
                    {MetricType.DagannothRex, DagannothRex},
                    {MetricType.DagannothSupreme, DagannothSupreme},
                    {MetricType.DerangedArchaeologist, DerangedArchaeologist},
                    {MetricType.GeneralGraardor, GeneralGraardor},
                    {MetricType.GiantMole, GiantMole},
                    {MetricType.GrotesqueGuardians, GrotesqueGuardians},
                    {MetricType.Hespori, Hespori},
                    {MetricType.KalphiteQueen, KalphiteQueen},
                    {MetricType.KingBlackDragon, KingBlackDragon},
                    {MetricType.Kraken, Kraken},
                    {MetricType.Kreearra, Kreearra},
                    {MetricType.KrilTsutsaroth, KrilTsutsaroth},
                    {MetricType.Mimic, Mimic},
                    {MetricType.Nightmare, Nightmare},
                    {MetricType.Obor, Obor},
                    {MetricType.Sarachnis, Sarachnis},
                    {MetricType.Scorpia, Scorpia},
                    {MetricType.Skotizo, Skotizo},
                    {MetricType.TheGauntlet, TheGauntlet},
                    {MetricType.TheCorruptedGauntlet, TheCorruptedGauntlet},
                    {MetricType.TheatreOfBlood, TheatreOfBlood},
                    {MetricType.ThermonuclearSmokeDevil, ThermonuclearSmokeDevil},
                    {MetricType.TzkalZuk, TzkalZuk},
                    {MetricType.TztokJad, TztokJad},
                    {MetricType.Venenatis, Venenatis},
                    {MetricType.Vetion, Vetion},
                    {MetricType.Vorkath, Vorkath},
                    {MetricType.Wintertodt, Wintertodt},
                    {MetricType.Zalcano, Zalcano},
                    {MetricType.Zulrah, Zulrah}
                };
            }
        }

        public List<DeltaInfo> AllDeltaInfos {
            get {
                return _allDeltaMetricsInfos ??= AllDeltaMetrics.Select(x => x.Value.ToDeltaInfo(x.Key)).ToList();
            }
        }
        public List<DeltaInfo> SkillDeltaInfos {
            get {
                return _skillDeltaMetricsInfos ??= SkillDeltaMetrics.Select(x => x.Value.ToDeltaInfo(x.Key)).ToList();
            }
        }
        public List<DeltaInfo> BossDeltaInfos {
            get {
                return _bossDeltaMetricsInfos ??= BossDeltaMetrics.Select(x => x.Value.ToDeltaInfo(x.Key)).ToList();
            }
        }

        public DeltaMetric DeltaFromType(MetricType type) {
            return AllDeltaMetrics.SingleOrDefault(x => x.Key == type).Value;
        }

        public DeltaInfo InfoFromType(MetricType type) {
            return AllDeltaInfos.SingleOrDefault(x => x.Type == type);
        }
    }
}