using DiscordBot.Commands.Interactive2.Base.Definitions;

namespace DiscordBot.Services.Interfaces;

public interface ICommandDefinitionProvider {
    Result<IEnumerable<ICommandDefinition>> GetAllDefinitions();
    Result<IEnumerable<(string Name, string Description)>> GetAllDefinitionDescriptions();
    Result<IEnumerable<(string Name, string Description)>> GetRootDefinitionDescriptions();
    Result<Dictionary<IRootCommandDefinition, ISubCommandDefinition[]>> GetRootDefinitionsWithSubDefinition();
    Result<IRootCommandDefinition> GetRootCommandByName(string command);
}
