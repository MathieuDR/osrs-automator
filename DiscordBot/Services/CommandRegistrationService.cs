using DiscordBot.Commands.Interactive;
using DiscordBot.Data.Interfaces;

namespace DiscordBot.Services;

public class CommandRegistrationService : ICommandRegistrationService {
    private readonly IApplicationCommandInfoRepository _applicationCommandInfoRepository;
    private readonly DiscordSocketClient _client;
    private readonly ILogger<CommandRegistrationService> _logger;
    private readonly ICommandStrategy _strategy;

    public CommandRegistrationService(ILogger<CommandRegistrationService> logger, ICommandStrategy strategy, DiscordSocketClient client,
        IApplicationCommandInfoRepository applicationCommandInfoRepository) {
        _logger = logger;
        _strategy = strategy;
        _client = client;
        _applicationCommandInfoRepository = applicationCommandInfoRepository;
    }


    public async Task<Result> UpdateCommand(IApplicationCommandHandler applicationCommandHandler, ApplicationCommandInfo applicationCommandInfo) {
        _logger.LogInformation("Updating command {command} with hash {hash}", applicationCommandHandler.Name, applicationCommandInfo.Hash);
        var currentHash = await applicationCommandHandler.GetCommandBuilderHash();
        var properties = await applicationCommandHandler.GetCommandProperties();

        if (currentHash != applicationCommandInfo.Hash) {
            _logger.LogInformation("Command {command} updated, new hash is {hash}", applicationCommandInfo.CommandName, currentHash);
        }

        await HandleGlobalRegistration(applicationCommandHandler, applicationCommandInfo, currentHash, properties);

        var guilds = _client.Guilds;
        await HandleGuildRegistries(applicationCommandHandler, applicationCommandInfo, guilds, currentHash, properties);

        if (currentHash != applicationCommandInfo.Hash) {
            // Hash is different, so lets update
            _logger.LogInformation("Updating command info for {name} command", applicationCommandInfo.CommandName);
            applicationCommandInfo = applicationCommandInfo with { Hash = currentHash };
            _applicationCommandInfoRepository.UpdateOrInsert(applicationCommandInfo);
        }

        return Result.Ok();
    }

    public async Task<Result> UpdateCommand(ApplicationCommandInfo applicationCommandInfo) {
        var handler = await _strategy.GetHandler(applicationCommandInfo.CommandName);
        return await UpdateCommand(handler, applicationCommandInfo);
    }

    public async Task<Result> UpdateAllCommands(IEnumerable<ApplicationCommandInfo> commandInfos) {
        // List<Result> results = new List<Result>();
        var commandInfoArr = commandInfos.ToArray();
        var tasks = new Task<Result>[commandInfoArr.Length];
        for (var i = 0; i < commandInfoArr.Length; i++) {
            var commandInfo = commandInfoArr[i];
            tasks[i] = UpdateCommand(commandInfo);
        }

        ResultBase[] results = await Task.WhenAll(tasks);
        return Result.Merge(results);
    }

    private async Task HandleGuildRegistries(IApplicationCommandHandler applicationCommandHandler,
        ApplicationCommandInfo applicationCommandInfo, IEnumerable<SocketGuild> guilds, uint currentHash, SlashCommandProperties properties) {
        foreach (var guild in guilds) {
            using var scope = _logger.BeginScope(new Dictionary<string, object> { { "Guild", new { guild.Id, guild.Name } } });
            var commands = await guild.GetApplicationCommandsAsync();
            var command = commands.FirstOrDefault(x => x.Name == applicationCommandHandler.Name && x.IsGlobalCommand == false);

            if (command is not null && currentHash != applicationCommandInfo.Hash) {
                _logger.LogInformation("Deleting {name} command because of new version", applicationCommandInfo.CommandName);
                await command.DeleteAsync();
                command = null;
            }

            if (applicationCommandInfo.RegisteredGuilds.Contains(guild.Id)) {
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

    private async Task HandleGlobalRegistration(IApplicationCommandHandler applicationCommandHandler,
        ApplicationCommandInfo applicationCommandInfo, uint currentHash, SlashCommandProperties properties) {
        // Get global commands
        var globalCommands = await _client.GetGlobalApplicationCommandsAsync();
        var globalCommand = globalCommands.FirstOrDefault(x => x.Name == applicationCommandHandler.Name);

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
