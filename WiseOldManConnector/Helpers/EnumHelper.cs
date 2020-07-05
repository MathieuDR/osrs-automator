using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Helpers {
    public static class EnumHelper {
        internal static string GetEnumValueNameOrDefault<T>(this T enumVal) {
            var enumType = typeof(T);
            var memInfo = enumType.GetMember(enumVal.ToString());
            var attr = memInfo.FirstOrDefault()?.GetCustomAttributes(false).OfType<EnumMemberAttribute>().FirstOrDefault();

            if (attr != null) { 
                return attr.Value;
            }

            return enumVal.ToString();
        }

        public static List<MetricType> GetMetricTypes(this MetricTypeCategory category) {
            switch (category) {
                case MetricTypeCategory.All:
                    var skills = GetMetricTypes(MetricTypeCategory.Skills);
                    var activities = GetMetricTypes(MetricTypeCategory.Activities);
                    var bosses = GetMetricTypes(MetricTypeCategory.Bosses);

                    var result =  new List<MetricType>();
                    result.AddRange(skills);
                    result.AddRange(activities);
                    result.AddRange(bosses);

                    return result;
                case MetricTypeCategory.Skills:
                    return SkillMetrics();
                case MetricTypeCategory.Bosses:
                    return BossMetrics();
                case MetricTypeCategory.Activities:
                    return ActivityMetrics();
                case MetricTypeCategory.Others:
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }
        }

        public static List<MetricType> BossMetrics() {
            return new List<MetricType>() {
                MetricType.AbyssalSire,
                MetricType.AlchemicalHydra,
                MetricType.BarrowsChests,
                MetricType.Bryophyta,
                MetricType.Callisto,
                MetricType.Cerberus,
                MetricType.ChambersOfXeric,
                MetricType.ChambersOfXericChallengeMode,
                MetricType.ChaosElemental,
                MetricType.ChaosFanatic,
                MetricType.CommanderZilyana,
                MetricType.CorporealBeast,
                MetricType.CrazyArchaeologist,
                MetricType.DagannothPrime,
                MetricType.DagannothRex,
                MetricType.DagannothSupreme,
                MetricType.DerangedArchaeologist,
                MetricType.GeneralGraardor,
                MetricType.GiantMole,
                MetricType.GrotesqueGuardians,
                MetricType.Hespori,
                MetricType.KalphiteQueen,
                MetricType.KingBlackDragon,
                MetricType.Kraken,
                MetricType.Kreearra,
                MetricType.KrilTsutsaroth,
                MetricType.Mimic,
                MetricType.Nightmare,
                MetricType.Obor,
                MetricType.Sarachnis,
                MetricType.Scorpia,
                MetricType.Skotizo,
                MetricType.TheGauntlet,
                MetricType.TheCorruptedGauntlet,
                MetricType.TheatreOfBlood,
                MetricType.ThermonuclearSmokeDevil,
                MetricType.TzkalZuk,
                MetricType.TztokJad,
                MetricType.Venenatis,
                MetricType.Vetion,
                MetricType.Vorkath,
                MetricType.Wintertodt,
                MetricType.Zalcano,
                MetricType.Zulrah,
            };
        }

        public static List<MetricType> ActivityMetrics() {
            return new List<MetricType>() {
                MetricType.LeaguePoints,
                MetricType.BountyHunterHunter,
                MetricType.BountyHunterRogue,
                MetricType.ClueScrollsAll,
                MetricType.ClueScrollsBeginner,
                MetricType.ClueScrollsEasy,
                MetricType.ClueScrollsMedium,
                MetricType.ClueScrollsHard,
                MetricType.ClueScrollsElite,
                MetricType.ClueScrollsMaster,
                MetricType.LastManStanding,
            };
        }

        public static List<MetricType> SkillMetrics() {
            return new List<MetricType>() {
                MetricType.Overall,
                MetricType.Attack,
                MetricType.Defence,
                MetricType.Strength,
                MetricType.Hitpoints,
                MetricType.Ranged,
                MetricType.Prayer,
                MetricType.Magic,
                MetricType.Cooking,
                MetricType.Woodcutting,
                MetricType.Fletching,
                MetricType.Fishing,
                MetricType.Firemaking,
                MetricType.Crafting,
                MetricType.Smithing,
                MetricType.Mining,
                MetricType.Herblore,
                MetricType.Agility,
                MetricType.Thieving,
                MetricType.Slayer,
                MetricType.Farming,
                MetricType.Runecrafting,
                MetricType.Hunter,
                MetricType.Construction,
            };
        }
    }
}