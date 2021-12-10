using Common.Extensions;
using DiscordBot.Commands.Interactive2.Base.Definitions;
using DiscordBot.Commands.Interactive2.Base.Requests;
using MediatR;

namespace DiscordBot.Services;

public class CommandInstigator : ICommandInstigator {
    private readonly Dictionary<IRootCommandDefinition, ISubCommandDefinition[]> _commands;
    private readonly IMediator _mediator;

    private readonly Dictionary<ICommandDefinition, Dictionary<Type, Type>> _commandRequests = new();

    public CommandInstigator(IMediator mediator,
        ICommandDefinitionProvider commandDefinitionProvider,
        IEnumerable<Type> requests) {
        
        _mediator = mediator;
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
                _commandRequests.Add(subCommand, GetTypeOfCommandRequestForCommandDefinition(subCommand.GetType(), requests));
            }
            _commandRequests.Add(commandBundle.Key, GetTypeOfCommandRequestForCommandDefinition(commandBundle.Key.GetType(), requests));
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="commandDefinition"></param>
    /// <param name="requests"></param>
    /// <returns>Dictionary of types. First type is a context type and the value is the request</returns>
    private Dictionary<Type, Type> GetTypeOfCommandRequestForCommandDefinition(Type commandDefinition, Type[] requests) {
       var requestsOfCommand =  requests.Where(x => x.GetGenericArguments().Contains(commandDefinition)).ToArray();
       var withContextType = requestsOfCommand.Select(requestType => (Context:requestType.GetGenericArguments().FirstOrDefault(generic => typeof(BaseInteractiveContext).IsAssignableFrom(generic)), requestType))
           .Where(item=>item.Context is not null).ToDictionary(x=> x.Context, x=>x.requestType);

       return withContextType;

    }

    public async Task<Result> ExecuteCommandAsync<T>(BaseInteractiveContext<T> context) where T : SocketInteraction {
        var commandDefinitionResult = GetCommandDefinition(context);

        if (commandDefinitionResult.IsFailed) {
            return Result.Fail("Could not execute command").WithErrors(commandDefinitionResult.Errors);
        }

        // Create command request from definition
        var request = CreateCommandRequest(context, commandDefinitionResult.Value);

        // execute command from definition through mediatr
        return await _mediator.Send(request);
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
        if (!_commandRequests.ContainsKey(definition)) {
            return null;
        }
        
        var dictionary = _commandRequests[definition];
        var requestType = dictionary[context.GetType()];
        if (requestType is null) {
            return null;
        }
        return (ICommandRequest<TContext>) Activator.CreateInstance(requestType, context);
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
