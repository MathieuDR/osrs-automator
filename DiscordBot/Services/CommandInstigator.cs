using DiscordBot.Commands.Interactive2.Base.Definitions;
using DiscordBot.Commands.Interactive2.Base.Requests;
using MediatR;

namespace DiscordBot.Services;

public interface ICommandInstigator {
    Task<Result> ExecuteCommandAsync<T>(BaseInteractiveContext<T> context) where T : SocketInteraction;
}

public class CommandInstigator : ICommandInstigator {
    private readonly (IRootCommandDefinition rootCommand, IEnumerable<ISubCommandDefinition> subCommands)[] _commands;
    private readonly IMediator _mediator;
    private readonly Type[] _requests;
    
    private readonly Dictionary<ICommandDefinition, Dictionary<Type, Type>> _commandRequests = new();

    public CommandInstigator(IMediator mediator,
        IEnumerable<(IRootCommandDefinition rootCommand, IEnumerable<ISubCommandDefinition> subCommands)> commands, 
        IEnumerable<Type> requests,
        IEnumerable<Type> contextTypes) {
        _mediator = mediator;
        _commands = commands.ToArray();
        _requests = requests.ToArray();

        InitializeCommandRequestDictionary(_commands, _requests);
    }
    
    private void InitializeCommandRequestDictionary((IRootCommandDefinition rootCommand, IEnumerable<ISubCommandDefinition> subCommands)[] commands, Type[] requests) {
        foreach (var commandBundle in commands) {
            foreach (var subCommand in commandBundle.subCommands) {
                // check if the requests has the subcommand type as generic type parameter
                _commandRequests.Add(subCommand, GetTypeOfCommandRequestForCommandDefinition(subCommand, requests));
            }
            _commandRequests.Add(commandBundle.rootCommand, GetTypeOfCommandRequestForCommandDefinition(commandBundle.rootCommand, requests));
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="commandDefinition"></param>
    /// <param name="requests"></param>
    /// <returns>Dictionary of types. First type is a context type and the value is the request</returns>
    private Dictionary<Type, Type> GetTypeOfCommandRequestForCommandDefinition(ICommandDefinition commandDefinition, Type[] requests) {
       var requestsOfCommand =  requests.Where(x => x.GetGenericArguments().Contains(commandDefinition.GetType())).ToArray();
       var withContextType = requestsOfCommand.Select(requestType => (Context:requestType.GetGenericArguments().FirstOrDefault(generic => typeof(BaseInteractiveContext).IsAssignableFrom(generic)), requestType))
           .Where(item=>item.Context is not null).ToDictionary(x=> x.Context, x=>x.requestType);

       return withContextType;

    }

    public async Task<Result> ExecuteCommandAsync<T>(BaseInteractiveContext<T> context) where T : SocketInteraction {
        var commandDefinitionResult = GetCommandDefinition(context);

        if (commandDefinitionResult.IsFailed) {
            return Result.Fail("Could not find command").WithErrors(commandDefinitionResult.Errors);
        }

        // Create command request from definition
        var request = CreateCommandRequest(context, commandDefinitionResult.Value);

        // execute command from definition through mediatr
        return await _mediator.Send(request);
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
        var commandDefinitions = _commands.Where(x => x.rootCommand.Name == context.Command).ToList();

        // Error handling
        if (commandDefinitions.Count == 0) {
            return Result.Fail<ICommandDefinition>("Could not find command");
        }

        if (commandDefinitions.Count > 1) {
            return Result.Fail<ICommandDefinition>("Found more then one command");
        }

        if (string.IsNullOrEmpty(context.SubCommand)) {
            var sub = commandDefinitions.First().subCommands.FirstOrDefault(x => x.Name == context.SubCommand);
            return sub == null ? Result.Fail<ICommandDefinition>("Could not find command") : Result.Ok<ICommandDefinition>(sub);
        }

        return Result.Ok<ICommandDefinition>(commandDefinitions.First().rootCommand);
    }
}
