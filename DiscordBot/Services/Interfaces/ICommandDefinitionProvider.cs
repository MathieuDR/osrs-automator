using DiscordBot.Commands.Interactive2.Base.Definitions;

namespace DiscordBot.Services.Interfaces;

public interface ICommandDefinitionProvider {
    Result<IEnumerable<ICommandDefinition>> GetAllDefinitions();
    Result<IEnumerable<(string name, string description)>> GetAllDefinitionDescriptions();
    Result<Dictionary<IRootCommandDefinition, ISubCommandDefinition[]>> GetRootDefinitionsWithSubDefinition();
}
