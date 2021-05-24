using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Helpers {
    public static class EnumHelper {
        public static string GetEnumValueNameOrDefault<T>(this T enumVal) {
            var enumType = typeof(T);
            var memInfo = enumType.GetMember(enumVal.ToString());
            var attr = memInfo.FirstOrDefault()?.GetCustomAttributes(false).OfType<EnumMemberAttribute>().FirstOrDefault();

            if (attr != null) { 
                return attr.Value;
            }

            return enumVal.ToString();
        }

        public static string FriendlyName(this MetricType metricType, bool capitalize = false) {
            var valName = metricType.GetEnumValueNameOrDefault();
            valName = valName.Replace('_', ' ');

            if (capitalize) {
                valName = $"{char.ToUpper(valName.First())}{valName.Substring(1)}";
            }

            return valName;
        }

        public static MetricTypeCategory Category(this MetricType metricType) {
            if (GetMetricTypes(MetricTypeCategory.Bosses).Contains(metricType)) {
                return MetricTypeCategory.Bosses;
            }

            if (GetMetricTypes(MetricTypeCategory.Skills).Contains(metricType)) {
                return MetricTypeCategory.Skills;
            }
            
            if (GetMetricTypes(MetricTypeCategory.Activities).Contains(metricType)) {
                return MetricTypeCategory.Activities;
            }

            if (GetMetricTypes(MetricTypeCategory.Time).Contains(metricType)) {
                return MetricTypeCategory.Time;
            }

            throw new ArgumentOutOfRangeException($"Cannot find category of {metricType}");
        }

        public static List<MetricType> GetMetricTypes(this MetricTypeCategory category) {
            switch (category) {
                case MetricTypeCategory.All:
                    var skills = GetMetricTypes(MetricTypeCategory.Skills);
                    var activities = GetMetricTypes(MetricTypeCategory.Activities);
                    var bosses = GetMetricTypes(MetricTypeCategory.Bosses);
                    var time = GetMetricTypes(MetricTypeCategory.Time);

                    var result =  new List<MetricType>();
                    result.AddRange(skills);
                    result.AddRange(activities);
                    result.AddRange(bosses);
                    result.AddRange(time);
                    return result;
                
                case MetricTypeCategory.Skills:
                    return SkillMetrics();
                case MetricTypeCategory.Activities:
                    return ActivityMetrics();
                case MetricTypeCategory.Bosses:
                    return BossMetrics();
                case MetricTypeCategory.Time:
                    return TimeMetrics();
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
                MetricType.Tempoross,
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
        
        public static List<MetricType> TimeMetrics() {
            return new List<MetricType>() {
                MetricType.EffectiveHoursBossing,
                MetricType.EffectiveHoursPlaying
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