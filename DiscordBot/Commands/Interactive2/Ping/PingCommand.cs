using DiscordBot.Commands.Interactive2.Interfaces;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Ping; 

public class PingCommand : ICommandDefinition {
    public string Name { get; }
    public string Description { get; }
    public AuthorizationRoles MinimumAuthorizationRole { get; }
    public Guid Id { get; }
    
    public Task<uint> GetCommandBuilderHash() {
        throw new NotImplementedException();
    }

    public Task<SlashCommandProperties> GetCommandProperties() {
        throw new NotImplementedException();
    }
}
