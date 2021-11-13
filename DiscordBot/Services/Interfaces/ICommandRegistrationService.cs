using DiscordBot.Commands.Interactive;

namespace DiscordBot.Services.Interfaces; 

public interface ICommandRegistrationService {
    // Task<Result> RegisterCommand(IApplicationCommandCommand applicationCommandCommand);
    // Task<Result> RegisterCommand(IApplicationCommandCommand applicationCommandCommand, ulong guildId);
    // Task<Result> UnregisterCommand(IApplicationCommandCommand applicationCommandCommand);
    // Task<Result> UnregisterCommand(IApplicationCommandCommand applicationCommandCommand, ulong guildId);
    Task<Result> UpdateCommand(IApplicationCommandHandler applicationCommandHandler, ApplicationCommandInfo applicationCommandInfo);
    Task<Result> UpdateCommand(ApplicationCommandInfo applicationCommandInfo);
    Task<Result> UpdateAllCommands(IEnumerable<ApplicationCommandInfo> commandInfos);
}