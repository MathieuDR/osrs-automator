using System.Collections.Generic;
using System.Linq;
using DiscordBotFanatic.Models.Enums;
using Newtonsoft.Json;

namespace DiscordBotFanatic.Models.WiseOldMan.Responses.Models {
    public class DeltaMetrics {
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

        public DeltaMetric MetricFromEnum(MetricType type) {
            return AllDeltaMetrics.SingleOrDefault(x => x.Key == type.ToString()).Value;
        }

        public Dictionary<string, DeltaMetric> AllDeltaMetrics {
            get {
                var result = new Dictionary<string, DeltaMetric>();

                result.Add(nameof(Overall), Overall);
                result.Add(nameof(Attack), Attack);
                result.Add(nameof(Defence), Defence);
                result.Add(nameof(Strength), Strength);
                result.Add(nameof(Hitpoints), Hitpoints);
                result.Add(nameof(Ranged), Ranged);
                result.Add(nameof(Prayer), Prayer);
                result.Add(nameof(Magic), Magic);
                result.Add(nameof(Cooking), Cooking);
                result.Add(nameof(Woodcutting), Woodcutting);
                result.Add(nameof(Fletching), Fletching);
                result.Add(nameof(Fishing), Fishing);
                result.Add(nameof(Firemaking), Firemaking);
                result.Add(nameof(Crafting), Crafting);
                result.Add(nameof(Smithing), Smithing);
                result.Add(nameof(Mining), Mining);
                result.Add(nameof(Herblore), Herblore);
                result.Add(nameof(Agility), Agility);
                result.Add(nameof(Thieving), Thieving);
                result.Add(nameof(Slayer), Slayer);
                result.Add(nameof(Farming), Farming);
                result.Add(nameof(Runecrafting), Runecrafting);
                result.Add(nameof(Hunter), Hunter);
                result.Add(nameof(Construction), Construction);

                result.Add(nameof(LeaguePoints), LeaguePoints);
                result.Add(nameof(BountyHunterHunter), BountyHunterHunter);
                result.Add(nameof(BountyHunterRogue), BountyHunterRogue);
                result.Add(nameof(ClueScrollsAll), ClueScrollsAll);
                result.Add(nameof(ClueScrollsEasy), ClueScrollsEasy);
                result.Add(nameof(ClueScrollsMedium), ClueScrollsMedium);
                result.Add(nameof(ClueScrollsHard), ClueScrollsHard);
                result.Add(nameof(ClueScrollsElite), ClueScrollsElite);
                result.Add(nameof(ClueScrollsMaster), ClueScrollsMaster);
                result.Add(nameof(LastManStanding), LastManStanding);
                result.Add(nameof(AbyssalSire), AbyssalSire);
                result.Add(nameof(AlchemicalHydra), AlchemicalHydra);
                result.Add(nameof(BarrowsChests), BarrowsChests);
                result.Add(nameof(Bryophyta), Bryophyta);
                result.Add(nameof(Callisto), Callisto);
                result.Add(nameof(Cerberus), Cerberus);
                result.Add(nameof(ChambersOfXeric), ChambersOfXeric);
                result.Add(nameof(ChambersOfXericChallengeMode), ChambersOfXericChallengeMode);
                result.Add(nameof(ChaosElemental), ChaosElemental);
                result.Add(nameof(ChaosFanatic), ChaosFanatic);
                result.Add(nameof(CommanderZilyana), CommanderZilyana);
                result.Add(nameof(CorporealBeast), CorporealBeast);
                result.Add(nameof(CrazyArchaeologist), CrazyArchaeologist);
                result.Add(nameof(DagannothPrime), DagannothPrime);
                result.Add(nameof(DagannothRex), DagannothRex);
                result.Add(nameof(DagannothSupreme), DagannothSupreme);
                result.Add(nameof(DerangedArchaeologist), DerangedArchaeologist);
                result.Add(nameof(GeneralGraardor), GeneralGraardor);
                result.Add(nameof(GiantMole), GiantMole);
                result.Add(nameof(GrotesqueGuardians), GrotesqueGuardians);
                result.Add(nameof(Hespori), Hespori);
                result.Add(nameof(KalphiteQueen), KalphiteQueen);
                result.Add(nameof(KingBlackDragon), KingBlackDragon);
                result.Add(nameof(Kraken), Kraken);
                result.Add(nameof(Kreearra), Kreearra);
                result.Add(nameof(KrilTsutsaroth), KrilTsutsaroth);
                result.Add(nameof(Mimic), Mimic);
                result.Add(nameof(Nightmare), Nightmare);
                result.Add(nameof(Obor), Obor);
                result.Add(nameof(Sarachnis), Sarachnis);
                result.Add(nameof(Scorpia), Scorpia);
                result.Add(nameof(Skotizo), Skotizo);
                result.Add(nameof(TheGauntlet), TheGauntlet);
                result.Add(nameof(TheCorruptedGauntlet), TheCorruptedGauntlet);
                result.Add(nameof(TheatreOfBlood), TheatreOfBlood);
                result.Add(nameof(ThermonuclearSmokeDevil), ThermonuclearSmokeDevil);
                result.Add(nameof(TzkalZuk), TzkalZuk);
                result.Add(nameof(TztokJad), TztokJad);
                result.Add(nameof(Venenatis), Venenatis);
                result.Add(nameof(Vetion), Vetion);
                result.Add(nameof(Vorkath), Vorkath);
                result.Add(nameof(Wintertodt), Wintertodt);
                result.Add(nameof(Zalcano), Zalcano);
                result.Add(nameof(Zulrah), Zulrah);

                return result;
            }
        }

         public Dictionary<string, DeltaMetric> SkillDeltaMetrics {
            get {
                var result = new Dictionary<string, DeltaMetric>();

                result.Add(nameof(Overall), Overall);
                result.Add(nameof(Attack), Attack);
                result.Add(nameof(Defence), Defence);
                result.Add(nameof(Strength), Strength);
                result.Add(nameof(Hitpoints), Hitpoints);
                result.Add(nameof(Ranged), Ranged);
                result.Add(nameof(Prayer), Prayer);
                result.Add(nameof(Magic), Magic);
                result.Add(nameof(Cooking), Cooking);
                result.Add(nameof(Woodcutting), Woodcutting);
                result.Add(nameof(Fletching), Fletching);
                result.Add(nameof(Fishing), Fishing);
                result.Add(nameof(Firemaking), Firemaking);
                result.Add(nameof(Crafting), Crafting);
                result.Add(nameof(Smithing), Smithing);
                result.Add(nameof(Mining), Mining);
                result.Add(nameof(Herblore), Herblore);
                result.Add(nameof(Agility), Agility);
                result.Add(nameof(Thieving), Thieving);
                result.Add(nameof(Slayer), Slayer);
                result.Add(nameof(Farming), Farming);
                result.Add(nameof(Runecrafting), Runecrafting);
                result.Add(nameof(Hunter), Hunter);
                result.Add(nameof(Construction), Construction);

                return result;
            }
        }

          public Dictionary<string, DeltaMetric> BossDeltaMetrics {
            get {
                var result = new Dictionary<string, DeltaMetric>();
                result.Add(nameof(LeaguePoints), LeaguePoints);
                result.Add(nameof(BountyHunterHunter), BountyHunterHunter);
                result.Add(nameof(BountyHunterRogue), BountyHunterRogue);
                result.Add(nameof(ClueScrollsAll), ClueScrollsAll);
                result.Add(nameof(ClueScrollsEasy), ClueScrollsEasy);
                result.Add(nameof(ClueScrollsMedium), ClueScrollsMedium);
                result.Add(nameof(ClueScrollsHard), ClueScrollsHard);
                result.Add(nameof(ClueScrollsElite), ClueScrollsElite);
                result.Add(nameof(ClueScrollsMaster), ClueScrollsMaster);
                result.Add(nameof(LastManStanding), LastManStanding);
                result.Add(nameof(AbyssalSire), AbyssalSire);
                result.Add(nameof(AlchemicalHydra), AlchemicalHydra);
                result.Add(nameof(BarrowsChests), BarrowsChests);
                result.Add(nameof(Bryophyta), Bryophyta);
                result.Add(nameof(Callisto), Callisto);
                result.Add(nameof(Cerberus), Cerberus);
                result.Add(nameof(ChambersOfXeric), ChambersOfXeric);
                result.Add(nameof(ChambersOfXericChallengeMode), ChambersOfXericChallengeMode);
                result.Add(nameof(ChaosElemental), ChaosElemental);
                result.Add(nameof(ChaosFanatic), ChaosFanatic);
                result.Add(nameof(CommanderZilyana), CommanderZilyana);
                result.Add(nameof(CorporealBeast), CorporealBeast);
                result.Add(nameof(CrazyArchaeologist), CrazyArchaeologist);
                result.Add(nameof(DagannothPrime), DagannothPrime);
                result.Add(nameof(DagannothRex), DagannothRex);
                result.Add(nameof(DagannothSupreme), DagannothSupreme);
                result.Add(nameof(DerangedArchaeologist), DerangedArchaeologist);
                result.Add(nameof(GeneralGraardor), GeneralGraardor);
                result.Add(nameof(GiantMole), GiantMole);
                result.Add(nameof(GrotesqueGuardians), GrotesqueGuardians);
                result.Add(nameof(Hespori), Hespori);
                result.Add(nameof(KalphiteQueen), KalphiteQueen);
                result.Add(nameof(KingBlackDragon), KingBlackDragon);
                result.Add(nameof(Kraken), Kraken);
                result.Add(nameof(Kreearra), Kreearra);
                result.Add(nameof(KrilTsutsaroth), KrilTsutsaroth);
                result.Add(nameof(Mimic), Mimic);
                result.Add(nameof(Nightmare), Nightmare);
                result.Add(nameof(Obor), Obor);
                result.Add(nameof(Sarachnis), Sarachnis);
                result.Add(nameof(Scorpia), Scorpia);
                result.Add(nameof(Skotizo), Skotizo);
                result.Add(nameof(TheGauntlet), TheGauntlet);
                result.Add(nameof(TheCorruptedGauntlet), TheCorruptedGauntlet);
                result.Add(nameof(TheatreOfBlood), TheatreOfBlood);
                result.Add(nameof(ThermonuclearSmokeDevil), ThermonuclearSmokeDevil);
                result.Add(nameof(TzkalZuk), TzkalZuk);
                result.Add(nameof(TztokJad), TztokJad);
                result.Add(nameof(Venenatis), Venenatis);
                result.Add(nameof(Vetion), Vetion);
                result.Add(nameof(Vorkath), Vorkath);
                result.Add(nameof(Wintertodt), Wintertodt);
                result.Add(nameof(Zalcano), Zalcano);
                result.Add(nameof(Zulrah), Zulrah);

                return result;
            }
        }
    }
}