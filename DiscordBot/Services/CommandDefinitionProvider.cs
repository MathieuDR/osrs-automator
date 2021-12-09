using Common.Extensions;
using DiscordBot.Commands.Interactive2.Base.Definitions;

namespace DiscordBot.Services;

public class CommandDefinitionProvider : ICommandDefinitionProvider {
    private Dictionary<Type, Type[]> _commandDefinitionTypeDictionary;
    private Dictionary<ICommandDefinition, ICommandDefinition[]> _commandsDictionary;

    public CommandDefinitionProvider(Type[] assemblyTypes) {
        _commandDefinitionTypeDictionary = SortCommandDefinitionTypesByRootCommand(assemblyTypes.GetConcreteClassFromType(typeof(ICommandDefinition)));
    }

    private void ActivateTypes(Dictionary<Type, IEnumerable<Type>> commandDefinitionTypeDictionary) {
        // Creates instances of all these commandDefinitions
        _commandsDictionary = commandDefinitionTypeDictionary
            .ToDictionary(x=> Activator.CreateInstance(x.Key).As<ICommandDefinition>(), 
                x=> x.Value.Select(x=>Activator.CreateInstance(x).As<ICommandDefinition>()).ToArray());
    }
    
    /// <summary>
    /// Sort commands by root and sub commands
    /// </summary>
    /// <param name="commands"></param>
    /// <returns></returns>
    private static Dictionary<Type, Type[]> SortCommandDefinitionTypesByRootCommand(Type[] commands) {
        // Split them up into root and sub commands through interface
        var rootCommands = commands.Where(x => typeof(IRootCommandDefinition).IsAssignableFrom(x)).ToArray();
        var subCommands = commands.Where(x => typeof(ISubCommandDefinition).IsAssignableFrom(x)).Where(x => x.GetInterfaces()
            .Any(y => y.IsGenericType && y.GetGenericTypeDefinition() == typeof(ISubCommandDefinition<>))).ToArray();

        // Place commands in a dictionary based on generic type
        var commandDictionary = new Dictionary<Type, Type[]>();
        foreach (var rootCommand in rootCommands) {
            // Get all subcommands that have a generic parameter of rootcommand
            var subCommandsOfRootCommandOfType = subCommands.Where(x => x.GetInterfaces()
                .Any(y => y.GetGenericArguments().Any(z => z == rootCommand))).ToArray();
            commandDictionary.Add(rootCommand, subCommandsOfRootCommandOfType);
        }

        return commandDictionary;
    }
    
    public Result<IEnumerable<ICommandDefinition>> GetAllDefinitions() {
        // We concat the subs to the root
        return _commandsDictionary.SelectMany(x => new []{x.Key}.Concat(x.Value)).ToResult();
    }

    public Result<IEnumerable<(string name, string description)>> GetAllDefinitionDescriptions() {
        return _commandsDictionary.SelectMany(x => new []{x.Key}.Concat(x.Value)).Select(x=> (x.Name, x.Description)).ToResult();
    }

    public Result<Dictionary<ICommandDefinition, ICommandDefinition[]>> GetRootDefinitionsWithSubDefinition() {
        return _commandsDictionary.ToResult();
    }
}
