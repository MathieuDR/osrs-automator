using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Serialization;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace WiseOldManConnector.Helpers;

public static class EnumHelper {

    /// <summary>
    /// Get the displayname of an enum value, if not found try EnumMemberAttribute and if not found return the name of the enum
    /// </summary>
    /// <param name="enumVal"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string ToDisplayNameOrFriendly<T>(this T enumVal) where T : Enum {
        var attr = enumVal.GetAttribute<DisplayAttribute>();
        return attr != null ? attr.Name : enumVal.ToEnumMemberOrDefault();
    }
    
    /// <summary>
    /// Get the EnumMember attribute of the enum value or the default value if not found
    /// </summary>
    /// <param name="enumVal"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static string ToEnumMemberOrDefault<T>(this T enumVal) where T : Enum {
        var attr = enumVal.GetAttribute<EnumMemberAttribute>();
        return attr != null ? attr.Value : enumVal.ToString();
    }
    
    /// <summary>
    ///     A generic extension method that aids in reflecting 
    ///     and retrieving any attribute that is applied to an `Enum`.
    /// </summary>
    public static TAttribute GetAttribute<TAttribute>(this Enum enumValue) 
        where TAttribute : Attribute
    {
        return enumValue.GetType()
            .GetMember(enumValue.ToString())
            .First()
            .GetCustomAttribute<TAttribute>();
    }

    public static T ToEnum<T>(this string source) {
        var enumType = typeof(T);
        foreach (var name in Enum.GetNames(enumType)) {
            var enumMemberAttribute = ((EnumMemberAttribute[])enumType.GetField(name).GetCustomAttributes(typeof(EnumMemberAttribute), true))
                .Single();
            if (enumMemberAttribute.Value.ToLowerInvariant() == source.ToLowerInvariant()) {
                return (T)Enum.Parse(enumType, name);
            }
        }

        return default;
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

        if (GetMetricTypes(MetricTypeCategory.Others).Contains(metricType)) {
            return MetricTypeCategory.Others;
        }

        throw new ArgumentOutOfRangeException($"Cannot find category of {metricType}");
    }

    public static List<MetricType> GetMetricTypes(this MetricTypeCategory category) {
        switch (category) {
            case MetricTypeCategory.All:
                var q = GetMetricTypes(MetricTypeCategory.Queryable);
                q.AddRange(GetMetricTypes(MetricTypeCategory.Others));
                return q;
            case MetricTypeCategory.Queryable:
                var result = new List<MetricType>();
                result.AddRange(GetMetricTypes(MetricTypeCategory.Skills));
                result.AddRange(GetMetricTypes(MetricTypeCategory.Activities));
                result.AddRange(GetMetricTypes(MetricTypeCategory.Bosses));
                result.AddRange(GetMetricTypes(MetricTypeCategory.Time));
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
                return OtherMetrics();
            default:
                throw new ArgumentOutOfRangeException(nameof(category), category, null);
        }
    }

    private static List<MetricType> BossMetrics() {
        return new List<MetricType> {
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
            MetricType.Nex,
            MetricType.Nightmare,
            MetricType.PhosanisNightmare,
            MetricType.Obor,
            MetricType.Sarachnis,
            MetricType.Scorpia,
            MetricType.Skotizo,
            MetricType.Tempoross,
            MetricType.TheGauntlet,
            MetricType.TheCorruptedGauntlet,
            MetricType.TheatreOfBlood,
            MetricType.TheatreOfBloodHardMode,
            MetricType.ThermonuclearSmokeDevil,
            MetricType.TombsOfAmascut,
            MetricType.TombsOfAmascutExpertMode,
            MetricType.TzkalZuk,
            MetricType.TztokJad,
            MetricType.Venenatis,
            MetricType.Vetion,
            MetricType.Vorkath,
            MetricType.Wintertodt,
            MetricType.Zalcano,
            MetricType.Zulrah
        };
    }

    private static List<MetricType> ActivityMetrics() {
        return new List<MetricType> {
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
            MetricType.SoulWarsZeal
        };
    }

    private static List<MetricType> TimeMetrics() {
        return new List<MetricType> {
            MetricType.EffectiveHoursBossing,
            MetricType.EffectiveHoursPlaying
        };
    }

    private static List<MetricType> SkillMetrics() {
        return new List<MetricType> {
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
            MetricType.Construction
        };
    }

    private static List<MetricType> OtherMetrics() {
        return new List<MetricType> {
            MetricType.Combat
        };
    }
}
