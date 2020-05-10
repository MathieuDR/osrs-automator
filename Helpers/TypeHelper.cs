using System;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Modules.Parameters;

namespace DiscordBotFanatic.Helpers {
    public static class TypeHelper {
        public static string ToHumanLanguage(this Type type, bool isSpecific = false) {

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = Nullable.GetUnderlyingType(type);
            }

            if (type == typeof(string)) {
                return "Text";
            }

            if (type == typeof(int)) {
                return "Number";
            }

            if (type == typeof(MetricOsrsArguments)) {
                return "Metric OSRS Arguments";
            }

            if (type == typeof(PeriodAndMetricOsrsArguments)) {
                return "Complete OSRS Arguments";
            }

            if (type == typeof(PeriodAndMetricOsrsArguments)) {
                return "Period OSRS Arguments";
            }

            if (isSpecific) {
                if (type == typeof(MetricType)) {
                    return "Metric Type";
                }

                if (type == typeof(Period)) {
                    return "Period";
                }
            }

            if (type.IsEnum) {
                return "Enumeration";
            }

            return "Unknown/Complex";
        }
    }
}