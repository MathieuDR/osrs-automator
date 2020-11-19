using System;
using AutoMapper;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Transformers.TypeConverters {
    internal class MetricTypeToDeltaTypeConverter : ITypeConverter<MetricType, DeltaType> {
        public DeltaType Convert(MetricType source, DeltaType destination, ResolutionContext context) {
            switch (source) {
                case MetricType.Overall:
                case MetricType.Attack:
                case MetricType.Defence:
                case MetricType.Strength:
                case MetricType.Hitpoints:
                case MetricType.Ranged:
                case MetricType.Prayer:
                case MetricType.Magic:
                case MetricType.Cooking:
                case MetricType.Woodcutting:
                case MetricType.Fletching:
                case MetricType.Fishing:
                case MetricType.Firemaking:
                case MetricType.Crafting:
                case MetricType.Smithing:
                case MetricType.Mining:
                case MetricType.Herblore:
                case MetricType.Agility:
                case MetricType.Thieving:
                case MetricType.Slayer:
                case MetricType.Farming:
                case MetricType.Runecrafting:
                case MetricType.Hunter:
                case MetricType.Construction:
                    return DeltaType.Experience;

                case MetricType.LeaguePoints:
                case MetricType.BountyHunterHunter:
                case MetricType.BountyHunterRogue:
                case MetricType.ClueScrollsAll:
                case MetricType.ClueScrollsBeginner:
                case MetricType.ClueScrollsEasy:
                case MetricType.ClueScrollsMedium:
                case MetricType.ClueScrollsHard:
                case MetricType.ClueScrollsElite:
                case MetricType.ClueScrollsMaster:
                case MetricType.LastManStanding:
                case MetricType.Combat:
                    return DeltaType.Score;

                case MetricType.AbyssalSire:
                case MetricType.AlchemicalHydra:
                case MetricType.BarrowsChests:
                case MetricType.Bryophyta:
                case MetricType.Callisto:
                case MetricType.Cerberus:
                case MetricType.ChambersOfXeric:
                case MetricType.ChambersOfXericChallengeMode:
                case MetricType.ChaosElemental:
                case MetricType.ChaosFanatic:
                case MetricType.CommanderZilyana:
                case MetricType.CorporealBeast:
                case MetricType.CrazyArchaeologist:
                case MetricType.DagannothPrime:
                case MetricType.DagannothRex:
                case MetricType.DagannothSupreme:
                case MetricType.DerangedArchaeologist:
                case MetricType.GeneralGraardor:
                case MetricType.GiantMole:
                case MetricType.GrotesqueGuardians:
                case MetricType.Hespori:
                case MetricType.KalphiteQueen:
                case MetricType.KingBlackDragon:
                case MetricType.Kraken:
                case MetricType.Kreearra:
                case MetricType.KrilTsutsaroth:
                case MetricType.Mimic:
                case MetricType.Nightmare:
                case MetricType.Obor:
                case MetricType.Sarachnis:
                case MetricType.Scorpia:
                case MetricType.Skotizo:
                case MetricType.TheGauntlet:
                case MetricType.TheCorruptedGauntlet:
                case MetricType.TheatreOfBlood:
                case MetricType.ThermonuclearSmokeDevil:
                case MetricType.TzkalZuk:
                case MetricType.TztokJad:
                case MetricType.Venenatis:
                case MetricType.Vetion:
                case MetricType.Vorkath:
                case MetricType.Wintertodt:
                case MetricType.Zalcano:
                case MetricType.Zulrah:
                    return DeltaType.Kills;

                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
        }
    }
}