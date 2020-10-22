using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Discord;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Modules.DiscordCommandArguments;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBotFanatic.Helpers {
    public static class TypeHelper {
        public static List<Type> WhiteListedTypesToOutput() {
            return new List<Type>() {
                typeof(BaseArguments),
                typeof(MetricArguments),
                typeof(PeriodArguments),
                typeof(PeriodAndMetricArguments),
                typeof(UserListWithImageArguments),
            };
        }

        public static string ToFriendlyExplenation(this Type type) {
            StringBuilder builder = new StringBuilder();
            bool identified = false;

            if (type == null) {
                builder.Append("Null");
                identified = true;
            }

            type = type.GetGenericInfo(builder);

            if (type == typeof(IUser) || type == typeof(IGuildUser)) {
                builder.Append( "Mention a(n) user(s) in the channel");
                identified = true;
            }

            if (type == typeof(IRole)) {
                builder.Append("Mention a role in the guild");
                identified = true;
            }

            if(!identified) {
                builder.Append("Unknown/Complex");
            }

            return builder.ToString();
        }

        private static Type GetGenericInfo(this Type type, StringBuilder builder = null) {
            builder ??= new StringBuilder();

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                builder.Append("Optional: ");
                return Nullable.GetUnderlyingType(type);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                builder.Append("List: ");
                return type.GetGenericArguments()[0];
            }

            return type;
        }

        public static string ToFriendlyName(this Type type, bool isSpecific = false) {
            StringBuilder builder = new StringBuilder();
            bool identified = false;

            if (type == null) {
                builder.Append("Null");
                identified = true;
            }

            type = type.GetGenericInfo(builder);

            if (type == typeof(string)) {
                builder.Append("Text");
                identified = true;
            }

            if (type == typeof(int)) {
                builder.Append("Number");
                identified = true;
            }

            if (type == typeof(MetricArguments)) {
                builder.Append("Metric OSRS Arguments");
                identified = true;
            }

            if (type == typeof(PeriodAndMetricArguments)) {
                builder.Append("Complete OSRS Arguments");
                identified = true;
            }

            if (type == typeof(PeriodAndMetricArguments)) {
                builder.Append("Period OSRS Arguments");
                identified = true;
            }

            if (type == typeof(UserListWithImageArguments)) {
                builder.Append("List of users and an image (with url or attachment)");
                identified = true;
            }

            if (type == typeof(IUser) || type == typeof(IGuildUser)) {
                builder.Append("Discord user");
                identified = true;
            }

            if (type == typeof(IRole)) {
                builder.Append("Guild role");
                identified = true;
            }

            if (isSpecific) {
                if (type == typeof(MetricType)) {
                    builder.Append("Metric Type");
                    identified = true;
                }

                if (type == typeof(Period)) {
                    builder.Append("Period");
                    identified = true;
                }
            }

            Debug.Assert(type != null, nameof(type) + " != null");
            if (type.IsEnum) {
                builder.Append("Enumeration");
                identified = true;
            }

            if(!identified) {
                builder.Append("Unknown/Complex");
            }

            return builder.ToString();
        }
    }
}