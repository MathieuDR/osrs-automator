using Common.Extensions;
using DiscordBot.Commands.Interactive2.Base.Definitions;
using DiscordBot.Commands.Interactive2.Base.Requests;
using MediatR;

namespace DiscordBot.Services;

public class CommandInstigator : ICommandInstigator {
    private readonly Dictionary<IRootCommandDefinition, ISubCommandDefinition[]> _commands;
    private readonly IMediator _mediator;
    private readonly ILogger<CommandInstigator> _logger;

    private readonly Dictionary<ICommandDefinition, Dictionary<Type, TypeHelper.ObjectActivator>> _commandRequestsActivators = new();

    public CommandInstigator(IMediator mediator,
        ICommandDefinitionProvider commandDefinitionProvider,
        IEnumerable<Type> requests, ILogger<CommandInstigator> logger) {
        
        _mediator = mediator;
        _logger = logger;
        _commands = GetCommandsFromProvider(commandDefinitionProvider);
        InitializeCommandRequestDictionary(_commands,  requests.ToArray());
    }

    private static Dictionary<IRootCommandDefinition, ISubCommandDefinition[]> GetCommandsFromProvider(ICommandDefinitionProvider commandDefinitionProvider) {
        var results = commandDefinitionProvider.GetRootDefinitionsWithSubDefinition();
        if (results.IsFailed) {
            throw new Exception(results.CombineMessage());
        }

        return results.Value;
    }

    private void InitializeCommandRequestDictionary(Dictionary<IRootCommandDefinition, ISubCommandDefinition[]> commands, Type[] requests) {
        foreach (var commandBundle in commands) {
            foreach (var subCommand in commandBundle.Value) {
                // check if the requests has the subcommand type as generic type parameter
                _commandRequestsActivators.Add(subCommand, GetTypeOfCommandRequestForCommandDefinition(subCommand.GetType(), requests));
            }
            _commandRequestsActivators.Add(commandBundle.Key, GetTypeOfCommandRequestForCommandDefinition(commandBundle.Key.GetType(), requests));
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="commandDefinition"></param>
    /// <param name="requests"></param>
    /// <returns>Dictionary of types. First type is a context type and the value is the request</returns>
    private Dictionary<Type, TypeHelper.ObjectActivator> GetTypeOfCommandRequestForCommandDefinition(Type commandDefinition, Type[] requests) {
        var requestsOfCommand =  requests.Where(x => x.GetInterfaces().Any(i => i.GetGenericArguments().Contains(commandDefinition))).ToArray();
        var result = new Dictionary<Type, TypeHelper.ObjectActivator>();

        foreach (var request in requestsOfCommand) {
            var contextType = request.GetInterfaces()
                .Select(i => i.GetGenericArguments().FirstOrDefault(generic => typeof(BaseInteractiveContext).IsAssignableFrom(generic)))
                .FirstOrDefault(t => t is not null);

            if (contextType is null) {
                continue;
            }
            
            var ctor = request.GetConstructors().First(x => x.GetParameters().Length == 1);
            var activator = ctor.GetActivator();
            
            result.Add(contextType, activator);
        }

    
        return result;
    }

    public async Task<Result> ExecuteCommandAsync<T>(BaseInteractiveContext<T> context) where T : SocketInteraction {
        var commandDefinitionResult = GetCommandDefinition(context);

        if (commandDefinitionResult.IsFailed) {
            return Result.Fail("Could not execute command").WithErrors(commandDefinitionResult.Errors);
        }

        // Create command request from definition
        var request = CreateCommandRequest(context, commandDefinitionResult.Value);

        // execute command from definition through mediatr
        try {
            // TODO Check if authorized!
            return await _mediator.Send(request);
        }catch(Exception e) {
            _logger.LogError(e, "Could not execute command");
            return Result.Fail("Could not execute command").WithError(new ExceptionalError(e));
        }
    }

    public Task<Result> ExecuteCommandAsync(BaseInteractiveContext context) {
        return context switch {
            ApplicationCommandContext appCtx => ExecuteCommandAsync(appCtx),
            AutocompleteCommandContext autocompleteCtx => ExecuteCommandAsync(autocompleteCtx),
            MessageComponentContext messageCtx => ExecuteCommandAsync(messageCtx),
            _ => throw new ArgumentOutOfRangeException(nameof(context), context, null)
        };
    }

    private ICommandRequest<TContext> CreateCommandRequest<TContext>(TContext context, ICommandDefinition definition) where TContext : BaseInteractiveContext {
        if (!_commandRequestsActivators.ContainsKey(definition)) {
            return null;
        }
        
        var dictionary = _commandRequestsActivators[definition];

        if (dictionary.TryGetValue(context.GetType(), out var activator)) {
            return activator(context).As<ICommandRequest<TContext>>();
        }

        return null;
    }

    private Result<ICommandDefinition> GetCommandDefinition<T>(BaseInteractiveContext<T> context) where T : SocketInteraction {
        var commandDefinitions = _commands.Where(x => x.Key.Name == context.Command).ToList();

        // Error handling
        if (commandDefinitions.Count == 0) {
            return Result.Fail<ICommandDefinition>("Could not find command");
        }

        if (commandDefinitions.Count > 1) {
            return Result.Fail<ICommandDefinition>("Found more then one command");
        }

        if (!string.IsNullOrEmpty(context.SubCommand)) {
            var sub = commandDefinitions.First().Value.FirstOrDefault(x => x.Name == context.SubCommand);
            return sub == null ? Result.Fail<ICommandDefinition>("Could not find subcommand") : Result.Ok<ICommandDefinition>(sub);
        }

        return Result.Ok<ICommandDefinition>(commandDefinitions.First().Key);
    }
}
