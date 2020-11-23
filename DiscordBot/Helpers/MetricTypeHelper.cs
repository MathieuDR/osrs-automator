using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using DiscordBotFanatic.Models.Configuration;
using DiscordBotFanatic.Models.Enums;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBotFanatic.Helpers {
    public static class MetricTypeHelper {
        public static bool TryParseToMetricType(this string metricType, MetricSynonymsConfiguration configuration,
            out object value) {
            if (Enum.TryParse(typeof(MetricType), metricType, true, out value)) {
                return true;
            }

            if (configuration != null && configuration.Data != null && configuration.Data.Count > 1) {
                foreach (KeyValuePair<MetricType, List<string>> kvp in configuration.Data) {
                    if (kvp.Value.Contains(metricType, StringComparer.InvariantCultureIgnoreCase)) {
                        value = kvp.Key;
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool IsBossMetric(this MetricType type) {
            return GetBossMetrics().Contains(type);
        }

        public static MetricType? GetSynonyms(string input) {
            return null;
        }

        public static bool IsActivityMetric(this MetricType type) {
            return GetActivityMetrics().Contains(type);
        }

        public static bool IsSkillMetric(this MetricType type) {
            return GetSkillMetrics().Contains(type);
        }

        public static List<MetricType> GetBossMetrics() {
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

        public static List<MetricType> GetActivityMetrics() {
            return new List<MetricType>() {
                MetricType.LeaguePoints,
                MetricType.BountyHunterHunter,
                MetricType.BountyHunterRogue,
                MetricType.ClueScrollsAll,
                MetricType.ClueScrollsEasy,
                MetricType.ClueScrollsMedium,
                MetricType.ClueScrollsHard,
                MetricType.ClueScrollsElite,
                MetricType.ClueScrollsMaster,
                MetricType.LastManStanding,
            };
        }

        public static List<MetricType> GetSkillMetrics() {
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

        public static string ToEmoji(this MetricType metric) {
            //return "";
            switch (metric) {
                case MetricType.Overall:
                    return "\uD83D\uDCCA";
                case MetricType.Attack:
                    return "\u2694";
                case MetricType.Defence:
                    return "\uD83D\uDEE1";
                case MetricType.Strength:
                    return "\uD83D\uDCAA";
                case MetricType.Hitpoints:
                    return "\u2764";
                case MetricType.Ranged:
                    return "\uD83C\uDFF9";
                case MetricType.Prayer:
                    return "\uD83E\uDDB4";
                case MetricType.Magic:
                    return "\uD83E\uDDD9";
                case MetricType.Cooking:
                    return "\uD83C\uDF6F";
                case MetricType.Woodcutting:
                    return "\uD83E\uDE93";
                case MetricType.Fletching:
                    return "\uD83D\uDE22";
                case MetricType.Fishing:
                    return "\uD83C\uDFA3";
                case MetricType.Firemaking:
                    return "\uD83D\uDD25";
                case MetricType.Crafting:
                    return "\u2692";
                case MetricType.Smithing:
                    return "\uD83D\uDD28";
                case MetricType.Mining:
                    return "\u26CF";
                case MetricType.Herblore:
                    return "\uD83C\uDF3F";
                case MetricType.Agility:
                    return "\uD83C\uDFC3";
                case MetricType.Thieving:
                    return "\uD83D\uDCB0";
                case MetricType.Slayer:
                    return "\uD83D\uDC80";
                case MetricType.Farming:
                    return "\uD83D\uDE9C";
                case MetricType.Runecrafting:
                    return "\uD83D\uDE2D";
                case MetricType.Hunter:
                    return "\uD83D\uDC26";
                case MetricType.Construction:
                    return "\uD83E\uDDF1";
                case MetricType.LeaguePoints:
                case MetricType.BountyHunterHunter:
                case MetricType.BountyHunterRogue:
                case MetricType.ClueScrollsAll:
                case MetricType.ClueScrollsEasy:
                case MetricType.ClueScrollsBeginner:
                case MetricType.ClueScrollsMedium:
                case MetricType.ClueScrollsHard:
                case MetricType.ClueScrollsElite:
                case MetricType.ClueScrollsMaster:
                case MetricType.LastManStanding:
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
                    return "";
                default:
                    throw new ArgumentOutOfRangeException(nameof(metric), metric, null);
            }
        }
    }
}