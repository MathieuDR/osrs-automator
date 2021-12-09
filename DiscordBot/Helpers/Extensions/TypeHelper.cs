using System.Diagnostics;
using System.Text;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Helpers.Extensions;

public static class TypeHelper {
    public static string ToFriendlyExplenation(this Type type) {
        var builder = new StringBuilder();
        var identified = false;

        if (type == null) {
            builder.Append("Null");
            identified = true;
        }

        type = type.GetGenericInfo(builder);

        if (type == typeof(IUser) || type == typeof(IGuildUser)) {
            builder.Append("Mention an user");
            identified = true;
        }

        if (type == typeof(IRole)) {
            builder.Append("Mention a role in the guild");
            identified = true;
        }

        if (type == typeof(IChannel)) {
            builder.Append("Mention a channel");
            identified = true;
        }

        if (!identified) {
            builder.Append("Unknown/Complex");
        }

        return builder.ToString();
    }

    public static string ToFriendlyName(this Type type, bool isSpecific = false) {
        var builder = new StringBuilder();
        var identified = false;

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

        if (type == typeof(IUser) || type == typeof(IGuildUser)) {
            builder.Append("Discord user");
            identified = true;
        }

        if (type == typeof(IChannel)) {
            builder.Append("Discord channel");
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

        if (!identified) {
            builder.Append("Unknown/Complex");
        }

        return builder.ToString();
    }

    private static Type GetGenericInfo(this Type type, StringBuilder builder = null) {
        builder ??= new StringBuilder();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) {
            builder.Append("Optional: ");
            return Nullable.GetUnderlyingType(type);
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
            builder.Append("List: ");
            return type.GetGenericArguments()[0];
        }

        return type;
    }
    
    /// <summary>
    /// Gets all types from a assemblies that implement a given interface and is not abstract
    /// </summary>
    /// <param name="assemblyTypes"></param>
    /// <param name="typeToScan"></param>
    /// <returns></returns>
    public static Type[] GetConcreteClassFromType(this Type[] assemblyTypes, Type typeToScan) {
        // Get assemblies from types
        var assemblies = assemblyTypes.Select(x => x.Assembly).Distinct().ToArray();

        // Get all commands from assemblies
        var foundTypes = assemblies.SelectMany(x => x.GetTypes())
            .Where(x => typeToScan.IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface)
            .ToArray();
        return foundTypes;
    }
    
    /// <summary>
    /// Gets all types from an assembly that implement a given interface and is not abstract
    /// </summary>
    /// <param name="assemblyType"></param>
    /// <param name="typeToScan"></param>
    /// <returns></returns>
    public static Type[] GetTypeFromTypes(this Type assemblyType, Type typeToScan) {
        return new[] { assemblyType }.GetConcreteClassFromType(typeToScan);
    }
}
