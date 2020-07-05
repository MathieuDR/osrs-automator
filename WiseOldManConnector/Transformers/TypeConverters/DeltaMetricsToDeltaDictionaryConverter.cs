using System;
using System.Collections.Generic;
using AutoMapper;
using WiseOldManConnector.Models.API.Responses.Models;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Transformers.TypeConverters {
    internal class DeltaMetricsToDeltaDictionaryConverter: ITypeConverter<DeltaMetrics, Dictionary<MetricType, DeltaMetric>> {
        public Dictionary<MetricType, DeltaMetric> Convert(DeltaMetrics source, Dictionary<MetricType, DeltaMetric> destination, ResolutionContext context) {
            destination = new Dictionary<MetricType, DeltaMetric>() {
                 {MetricType.Overall, context.Mapper.Map<DeltaMetric>(source.Overall)},
                {MetricType.Attack, context.Mapper.Map<DeltaMetric>(source.Attack)},
                {MetricType.Defence, context.Mapper.Map<DeltaMetric>(source.Defence)},
                {MetricType.Strength, context.Mapper.Map<DeltaMetric>(source.Strength)},
                {MetricType.Hitpoints, context.Mapper.Map<DeltaMetric>(source.Hitpoints)},
                {MetricType.Ranged, context.Mapper.Map<DeltaMetric>(source.Ranged)},
                {MetricType.Prayer, context.Mapper.Map<DeltaMetric>(source.Prayer)},
                {MetricType.Magic, context.Mapper.Map<DeltaMetric>(source.Magic)},
                {MetricType.Cooking, context.Mapper.Map<DeltaMetric>(source.Cooking)},
                {MetricType.Woodcutting, context.Mapper.Map<DeltaMetric>(source.Woodcutting)},
                {MetricType.Fletching, context.Mapper.Map<DeltaMetric>(source.Fletching)},
                {MetricType.Fishing, context.Mapper.Map<DeltaMetric>(source.Fishing)},
                {MetricType.Firemaking, context.Mapper.Map<DeltaMetric>(source.Firemaking)},
                {MetricType.Crafting, context.Mapper.Map<DeltaMetric>(source.Crafting)},
                {MetricType.Smithing, context.Mapper.Map<DeltaMetric>(source.Smithing)},
                {MetricType.Mining, context.Mapper.Map<DeltaMetric>(source.Mining)},
                {MetricType.Herblore, context.Mapper.Map<DeltaMetric>(source.Herblore)},
                {MetricType.Agility, context.Mapper.Map<DeltaMetric>(source.Agility)},
                {MetricType.Thieving, context.Mapper.Map<DeltaMetric>(source.Thieving)},
                {MetricType.Slayer, context.Mapper.Map<DeltaMetric>(source.Slayer)},
                {MetricType.Farming, context.Mapper.Map<DeltaMetric>(source.Farming)},
                {MetricType.Runecrafting, context.Mapper.Map<DeltaMetric>(source.Runecrafting)},
                {MetricType.Hunter, context.Mapper.Map<DeltaMetric>(source.Hunter)},
                {MetricType.Construction, context.Mapper.Map<DeltaMetric>(source.Construction)},
                {MetricType.LeaguePoints, context.Mapper.Map<DeltaMetric>(source.LeaguePoints)},
                {MetricType.BountyHunterHunter, context.Mapper.Map<DeltaMetric>(source.BountyHunterHunter)},
                {MetricType.BountyHunterRogue, context.Mapper.Map<DeltaMetric>(source.BountyHunterRogue)},
                {MetricType.ClueScrollsAll, context.Mapper.Map<DeltaMetric>(source.ClueScrollsAll)},
                {MetricType.ClueScrollsBeginner, context.Mapper.Map<DeltaMetric>(source.ClueScrollsBeginner)},
                {MetricType.ClueScrollsEasy, context.Mapper.Map<DeltaMetric>(source.ClueScrollsEasy)},
                {MetricType.ClueScrollsMedium, context.Mapper.Map<DeltaMetric>(source.ClueScrollsMedium)},
                {MetricType.ClueScrollsHard, context.Mapper.Map<DeltaMetric>(source.ClueScrollsHard)},
                {MetricType.ClueScrollsElite, context.Mapper.Map<DeltaMetric>(source.ClueScrollsElite)},
                {MetricType.ClueScrollsMaster, context.Mapper.Map<DeltaMetric>(source.ClueScrollsMaster)},
                {MetricType.LastManStanding, context.Mapper.Map<DeltaMetric>(source.LastManStanding)},
                {MetricType.AbyssalSire, context.Mapper.Map<DeltaMetric>(source.AbyssalSire)},
                {MetricType.AlchemicalHydra, context.Mapper.Map<DeltaMetric>(source.AlchemicalHydra)},
                {MetricType.BarrowsChests, context.Mapper.Map<DeltaMetric>(source.BarrowsChests)},
                {MetricType.Bryophyta, context.Mapper.Map<DeltaMetric>(source.Bryophyta)},
                {MetricType.Callisto, context.Mapper.Map<DeltaMetric>(source.Callisto)},
                {MetricType.Cerberus, context.Mapper.Map<DeltaMetric>(source.Cerberus)},
                {MetricType.ChambersOfXeric, context.Mapper.Map<DeltaMetric>(source.ChambersOfXeric)},
                {MetricType.ChambersOfXericChallengeMode, context.Mapper.Map<DeltaMetric>(source.ChambersOfXericChallengeMode)},
                {MetricType.ChaosElemental, context.Mapper.Map<DeltaMetric>(source.ChaosElemental)},
                {MetricType.ChaosFanatic, context.Mapper.Map<DeltaMetric>(source.ChaosFanatic)},
                {MetricType.CommanderZilyana, context.Mapper.Map<DeltaMetric>(source.CommanderZilyana)},
                {MetricType.CorporealBeast, context.Mapper.Map<DeltaMetric>(source.CorporealBeast)},
                {MetricType.CrazyArchaeologist, context.Mapper.Map<DeltaMetric>(source.CrazyArchaeologist)},
                {MetricType.DagannothPrime, context.Mapper.Map<DeltaMetric>(source.DagannothPrime)},
                {MetricType.DagannothRex, context.Mapper.Map<DeltaMetric>(source.DagannothRex)},
                {MetricType.DagannothSupreme, context.Mapper.Map<DeltaMetric>(source.DagannothSupreme)},
                {MetricType.DerangedArchaeologist, context.Mapper.Map<DeltaMetric>(source.DerangedArchaeologist)},
                {MetricType.GeneralGraardor, context.Mapper.Map<DeltaMetric>(source.GeneralGraardor)},
                {MetricType.GiantMole, context.Mapper.Map<DeltaMetric>(source.GiantMole)},
                {MetricType.GrotesqueGuardians, context.Mapper.Map<DeltaMetric>(source.GrotesqueGuardians)},
                {MetricType.Hespori, context.Mapper.Map<DeltaMetric>(source.Hespori)},
                {MetricType.KalphiteQueen, context.Mapper.Map<DeltaMetric>(source.KalphiteQueen)},
                {MetricType.KingBlackDragon, context.Mapper.Map<DeltaMetric>(source.KingBlackDragon)},
                {MetricType.Kraken, context.Mapper.Map<DeltaMetric>(source.Kraken)},
                {MetricType.Kreearra, context.Mapper.Map<DeltaMetric>(source.Kreearra)},
                {MetricType.KrilTsutsaroth, context.Mapper.Map<DeltaMetric>(source.KrilTsutsaroth)},
                {MetricType.Mimic, context.Mapper.Map<DeltaMetric>(source.Mimic)},
                {MetricType.Nightmare, context.Mapper.Map<DeltaMetric>(source.Nightmare)},
                {MetricType.Obor, context.Mapper.Map<DeltaMetric>(source.Obor)},
                {MetricType.Sarachnis, context.Mapper.Map<DeltaMetric>(source.Sarachnis)},
                {MetricType.Scorpia, context.Mapper.Map<DeltaMetric>(source.Scorpia)},
                {MetricType.Skotizo, context.Mapper.Map<DeltaMetric>(source.Skotizo)},
                {MetricType.TheGauntlet, context.Mapper.Map<DeltaMetric>(source.TheGauntlet)},
                {MetricType.TheCorruptedGauntlet, context.Mapper.Map<DeltaMetric>(source.TheCorruptedGauntlet)},
                {MetricType.TheatreOfBlood, context.Mapper.Map<DeltaMetric>(source.TheatreOfBlood)},
                {MetricType.ThermonuclearSmokeDevil, context.Mapper.Map<DeltaMetric>(source.ThermonuclearSmokeDevil)},
                {MetricType.TzkalZuk, context.Mapper.Map<DeltaMetric>(source.TzkalZuk)},
                {MetricType.TztokJad, context.Mapper.Map<DeltaMetric>(source.TztokJad)},
                {MetricType.Venenatis, context.Mapper.Map<DeltaMetric>(source.Venenatis)},
                {MetricType.Vetion, context.Mapper.Map<DeltaMetric>(source.Vetion)},
                {MetricType.Vorkath, context.Mapper.Map<DeltaMetric>(source.Vorkath)},
                {MetricType.Wintertodt, context.Mapper.Map<DeltaMetric>(source.Wintertodt)},
                {MetricType.Zalcano, context.Mapper.Map<DeltaMetric>(source.Zalcano)},
                {MetricType.Zulrah, context.Mapper.Map<DeltaMetric>(source.Zulrah)}
            };

            // Set correct key!
            foreach (KeyValuePair<MetricType, DeltaMetric> kvp in destination) {
                kvp.Value.MetricType = kvp.Key;
            }

            return destination;
        }
    }
}