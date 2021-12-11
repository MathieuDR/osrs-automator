using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
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

        // Get all types
        var foundTypes = assemblies.SelectMany(x => x.GetTypes())
            // Check if it's a concrete class (no abstract & no interface
            .Where(x=> !x.IsAbstract && !x.IsInterface)
            // Check is is not null, or is a concrete class that we can assign from x OR if it's a generic that is equal to the type definition
            .Where(x => x != null && (typeToScan.IsAssignableFrom(x) || x.GetInterfaces().Any(generic=> generic.IsGenericType && generic.GetGenericTypeDefinition() == typeToScan)))
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
    
    public delegate T ObjectActivator<T>(params object[] args);
    
    /// <summary>
    /// Linq expression trees to replace Activator.CreateInstance.
    /// </summary>
    /// <param name="ctor"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ObjectActivator<T> GetActivator<T>(this ConstructorInfo ctor) {
        Type type = ctor.DeclaringType;
        ParameterInfo[] paramsInfo = ctor.GetParameters();                  

        //create a single param of type object[]
        ParameterExpression param =
            Expression.Parameter(typeof(object[]), "args");
 
        Expression[] argsExp =
            new Expression[paramsInfo.Length];            

        //pick each arg from the params array 
        //and create a typed expression of them
        for (int i = 0; i < paramsInfo.Length; i++)
        {
            Expression index = Expression.Constant(i);
            Type paramType = paramsInfo[i].ParameterType;              

            Expression paramAccessorExp =
                Expression.ArrayIndex(param, index);              

            Expression paramCastExp =
                Expression.Convert (paramAccessorExp, paramType);              

            argsExp[i] = paramCastExp;
        }                  

        //make a NewExpression that calls the
        //ctor with the args we just created
        NewExpression newExp = Expression.New(ctor,argsExp);                  

        //create a lambda with the New
        //Expression as body and our param object[] as arg
        LambdaExpression lambda =
            Expression.Lambda(typeof(ObjectActivator<T>), newExp, param);              

        //compile it
        ObjectActivator<T> compiled = (ObjectActivator<T>)lambda.Compile();
        return compiled;
    }
}
