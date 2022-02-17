using DiscordBot.Commands.Interactive2.Base.Definitions;
using DiscordBot.Common.Models.Data.Configuration;
using DiscordBot.Data.Interfaces;

namespace DiscordBot.Services;

public class CommandDefinitionRegistrationService : ICommandRegistrationService {
    private readonly IApplicationCommandInfoRepository _applicationCommandInfoRepository;
    private readonly ICommandRegistrationService _oldRegistration;
    private readonly Dictionary<IRootCommandDefinition, ISubCommandDefinition[]> _commandDefinitions;
    private readonly DiscordSocketClient _client;
    private readonly ILogger<CommandDefinitionRegistrationService> _logger;

    public CommandDefinitionRegistrationService(ILogger<CommandDefinitionRegistrationService> logger, DiscordSocketClient client,
        IApplicationCommandInfoRepository applicationCommandInfoRepository,
        ICommandDefinitionProvider provider,
        ICommandRegistrationService oldRegistration) {
        _logger = logger;
        _client = client;
        _applicationCommandInfoRepository = applicationCommandInfoRepository;
        _oldRegistration = oldRegistration;
        _commandDefinitions = provider.GetRootDefinitionsWithSubDefinition().Value;
    }


    private async Task<Result> UpdateCommand((IRootCommandDefinition rootCommandDefinition, IEnumerable<ISubCommandDefinition> subCommandDefinitions) definitions, ApplicationCommandInfo applicationCommandInfo) {
        _logger.LogInformation("Updating command {command} with hash {hash}", applicationCommandInfo.CommandName, applicationCommandInfo.Hash);
        var currentHash = await definitions.rootCommandDefinition.GetCommandBuilderHash();
        var properties = await  definitions.rootCommandDefinition.GetCommandProperties();

        if (currentHash != applicationCommandInfo.Hash) {
            _logger.LogInformation("Command {command} updated, new hash is {hash}", applicationCommandInfo.CommandName, currentHash);
        }

        await HandleGlobalRegistration(applicationCommandInfo, currentHash, properties);

        var guilds = _client.Guilds;
        await HandleGuildRegistries(applicationCommandInfo, guilds, currentHash, properties);

        if (currentHash != applicationCommandInfo.Hash) {
            // Hash is different, so lets update
            _logger.LogInformation("Updating command info for {name} command", applicationCommandInfo.CommandName);
            applicationCommandInfo = applicationCommandInfo with { Hash = currentHash };
            _applicationCommandInfoRepository.UpdateOrInsert(applicationCommandInfo);
        }

        return Result.Ok();
    }

    public async Task<Result> UpdateCommand(ApplicationCommandInfo applicationCommandInfo) {
        var definitions = _commandDefinitions.FirstOrDefault(x => x.Key.Name == applicationCommandInfo.CommandName);
        if (definitions.Key == null) {
            return await _oldRegistration.UpdateCommand(applicationCommandInfo);
        }
        return await UpdateCommand((rootCommand: definitions.Key, subCommands: definitions.Value), applicationCommandInfo);
    }

    public async Task<Result> UpdateAllCommands(IEnumerable<ApplicationCommandInfo> commandInfos) {
        // List<Result> results = new List<Result>();
        var commandInfoArr = commandInfos.ToArray();
        var tasks = new Task<Result>[commandInfoArr.Length + 1];
        for (var i = 0; i < commandInfoArr.Length; i++) {
            var commandInfo = commandInfoArr[i];
            tasks[i] = UpdateCommand(commandInfo);
        }

        tasks[^1] = _oldRegistration.UpdateAllCommands(commandInfoArr);
        ResultBase[] results = await Task.WhenAll(tasks);

        return Result.Merge(results);
    }

    private async Task HandleGuildRegistries(ApplicationCommandInfo applicationCommandInfo, IEnumerable<SocketGuild> guilds, uint currentHash, SlashCommandProperties properties) {
        foreach (var guild in guilds) {
            using var scope = _logger.BeginScope(new Dictionary<string, object> { { "Guild", new { guild.Id, guild.Name } } });
            var commands = await guild.GetApplicationCommandsAsync();
            var command = commands.FirstOrDefault(x => x.Name == applicationCommandInfo.CommandName && x.IsGlobalCommand == false);

            if (command is not null && currentHash != applicationCommandInfo.Hash) {
                _logger.LogInformation("Deleting {name} command because of new version", applicationCommandInfo.CommandName);
                await command.DeleteAsync();
                command = null;
            }

            if (applicationCommandInfo.RegisteredGuilds.Contains(guild.GetGuildId())) {
                if (command is null) {
                    _logger.LogInformation("Adding {name} command", applicationCommandInfo.CommandName);
                    await guild.CreateApplicationCommandAsync(properties);
                }
            } else {
                if (command is not null) {
                    _logger.LogInformation("Deleting {name} command", applicationCommandInfo.CommandName);
                    await command.DeleteAsync();
                }
            }

            scope.Dispose();
        }
    }

    private async Task HandleGlobalRegistration(ApplicationCommandInfo applicationCommandInfo, uint currentHash, SlashCommandProperties properties) {
        // Get global commands
        var globalCommands = await _client.GetGlobalApplicationCommandsAsync();
        var globalCommand = globalCommands.FirstOrDefault(x => x.Name == applicationCommandInfo.CommandName);

        // If hashes aren't equal, we need to delete all
        if (currentHash != applicationCommandInfo.Hash) {
            if (globalCommand is not null) {
                _logger.LogInformation("Deleting global {name} command because of new version", applicationCommandInfo.CommandName);
                await globalCommand.DeleteAsync();
                globalCommand = null; // reset so it's 'not existing' later
            }
        }

        // register if it doesn't exist
        if (globalCommand is null && applicationCommandInfo.IsGlobal) {
            _logger.LogInformation("Adding global {name} command", applicationCommandInfo.CommandName);
            await _client.CreateGlobalApplicationCommandAsync(properties);
        } else if (globalCommand is not null && !applicationCommandInfo.IsGlobal) {
            _logger.LogInformation("Deleting global {name} command", applicationCommandInfo.CommandName);
            await globalCommand.DeleteAsync();
        }
    }
}
