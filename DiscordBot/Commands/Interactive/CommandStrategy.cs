
namespace DiscordBot.Commands.Interactive;

public class CommandStrategy : ICommandStrategy {
    private readonly IApplicationCommandHandler[] _commands;
    private readonly ICommandAuthorizationService _authorizationService;

    private readonly ILogger<CommandStrategy> _logger;

    public CommandStrategy(ILogger<CommandStrategy> logger, IApplicationCommandHandler[] commands,ICommandAuthorizationService authorizationService) {
        _logger = logger;
        _commands = commands ?? throw new ArgumentNullException(nameof(commands));
        _authorizationService = authorizationService;
    }



    public async Task<Result> HandleInteractiveCommand(BaseInteractiveContext context) {
        try {
            var result = context switch {
                MessageComponentContext messageComponentContext => HandleComponentCommand(messageComponentContext),
                ApplicationCommandContext applicationCommandContext => HandleApplicationCommand(applicationCommandContext),
                AutocompleteCommandContext autocompleteCommandContext => HandleAutoComplete(autocompleteCommandContext),
                _ => Task.FromResult(Result.Fail("Could not find context type"))
            };

            return await result;
        } catch (Exception e) {
            var guid = Guid.NewGuid();
            _logger.LogError(e, "Error handling command, trace {guid}", guid);
            return Result.Fail($"Error handling command. Trace: {guid}");
        }
    }

    public Task<IApplicationCommandHandler> GetHandler(string applicationCommandName) {
        return Task.FromResult(_commands.FirstOrDefault(c =>
            string.Equals(c.Name, applicationCommandName, StringComparison.InvariantCultureIgnoreCase)));
    }

    public async Task<Result> HandleAutoComplete(AutocompleteCommandContext context) {
        var command = _commands.FirstOrDefault(c => c.CanHandle(context));

        if (command is null) {
            return Result.Fail(new Error("Could not find proper command handler").WithMetadata("404", true));
        }

        if (!await _authorizationService.IsAuthorized(context, command)) {
            return Result.Fail(new Error("You are not authorized to use this command").WithMetadata("401", true));
        }

        return await command.HandleAutocompleteAsync(context);
    }

    
    public async Task<Result> HandleApplicationCommand(ApplicationCommandContext context) {
        var command = _commands.FirstOrDefault(c => c.CanHandle(context));

        if (command is null) {
            return Result.Fail(new Error("Could not find proper command handler").WithMetadata("404", true));
        }

        if (!await _authorizationService.IsAuthorized(context, command)) {
            return Result.Fail(new Error("You are not authorized to use this command").WithMetadata("401", true));
        }

        return await command.HandleCommandAsync(context);
    }

    public async Task<Result> HandleComponentCommand(MessageComponentContext context) {
        var command = _commands.FirstOrDefault(c => c.CanHandle(context));

        if (command is null) {
            return Result.Fail(new Error("Could not find proper component handler").WithMetadata("404", true));
        }

        if (!await _authorizationService.IsAuthorized(context, command)) {
            return Result.Fail(new Error("You are not authorized to use this command").WithMetadata("401", true));
        }

        return await command.HandleComponentAsync(context);
    }

    /// <summary>
    ///     Get all the command builders
    /// </summary>
    /// <param name="allBuilders">Retrieve all commands, even if they're not set to 'global'</param>
    /// <returns>SlashCommandProperties in the strategy</returns>
    public async Task<SlashCommandProperties[]> GetCommandPropertiesCollection(bool allBuilders = false) {
        var commandsToRetrieve = _commands.Where(c => c.GlobalRegister || allBuilders).ToList();

        var tasks = new Task<SlashCommandProperties>[commandsToRetrieve.Count];
        for (var i = 0; i < commandsToRetrieve.Count; i++) {
            var command = commandsToRetrieve[i];
            var builderTask = command.GetCommandProperties();
            tasks[i] = builderTask;
        }

        return await Task.WhenAll(tasks);
    }

    public async Task<SlashCommandProperties> GetCommandProperties(string applicationCommandName) {
        var command = await GetHandler(applicationCommandName);

        if (command is null) {
            return null;
        }

        return await command.GetCommandProperties();
    }

    public Task<uint> GetCommandHash(string applicationCommandName) {
        var command = _commands.FirstOrDefault(c => string.Equals(c.Name, applicationCommandName, StringComparison.InvariantCultureIgnoreCase));

        if (command is null) {
            return null;
        }

        return command.GetCommandBuilderHash();
    }

    public IEnumerable<(string Name, string Description)> GetCommandDescriptions() {
        return _commands.Select(c => (c.Name, c.Description));
    }
}
