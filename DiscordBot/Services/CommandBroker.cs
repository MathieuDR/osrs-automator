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
    private readonly ICommandRequest<BaseInteractiveContext>[] _requests;
    
    private readonly Dictionary<ICommandDefinition, Dictionary<ContextType, Type> _commandRequests = new();

    public CommandInstigator(IMediator mediator,
        IEnumerable<(IRootCommandDefinition rootCommand, IEnumerable<ISubCommandDefinition> subCommands)> commands, IEnumerable<ICommandRequest<BaseInteractiveContext>> requests) {
        _mediator = mediator;
        _commands = commands.ToArray();
        _requests = requests.ToArray();

        InitializeCommandRequestDictionary(_commands, _requests);
    }
    
    
    // Event -> Interaction -> Context (Which i have 3 types of)
    // Context -> CommandDefinition -> Dictionary which has ContextType as key and A Mediatr Request as value 

    private void InitializeCommandRequestDictionary((IRootCommandDefinition rootCommand, IEnumerable<ISubCommandDefinition> subCommands)[] commands, ICommandRequest<BaseInteractiveContext>[] requests) {
        foreach (var rootCommand in commands) {
            foreach (var subCommand in rootCommand.subCommands) {
                // check if the requests if it has the subcommand type as generic type parameter
                var request = requests.Where(x => x.GetType().GetGenericArguments().Contains(subCommand.GetType())).ToList();
            }
        }
    }
    
    private IEnumerable<ICommandRequest<BaseInteractiveContext>> GetRequests(ICommandDefinition command) {
        if (_commandRequests.TryGetValue(command, out var requests)) {
            return requests.Values;
        }

        return Enumerable.Empty<ICommandRequest<BaseInteractiveContext>>();
    }


    public async Task<Result> ExecuteCommandAsync<T>(BaseInteractiveContext<T> context) where T : SocketInteraction {
        var commandDefinitionResult = GetCommandDefinition(context);

        if (commandDefinitionResult.IsFailed) {
            return Result.Fail("Could not find command").WithErrors(commandDefinitionResult.Errors);
        }

        // Create command request from definition
        var request = CreateCommandRequest(context);

        // execute command from definition through mediatr
        return await _mediator.Send(request);
    }

    private static ICommandRequest<TContext> CreateCommandRequest<TContext>(TContext context) where TContext : BaseInteractiveContext {
        throw new NotImplementedException();
    }

    private Result<ICommandDefinition> GetCommandDefinition<T>(BaseInteractiveContext<T> context) where T : SocketInteraction {
        var commandDefinitions = _commands.Where(x => x.rootCommand.Name == context.Command).ToList();

        // Error handling
        switch (commandDefinitions.Count) {
            case 0:
                return Result.Fail<ICommandDefinition>("Could not find command");
            case > 1:
                return Result.Fail<ICommandDefinition>("Found more then one command");
        }

        if (string.IsNullOrEmpty(context.SubCommand)) {
            var sub = commandDefinitions.First().subCommands.FirstOrDefault(x => x.Name == context.SubCommand);
            return sub == null ? Result.Fail<ICommandDefinition>("Could not find command") : Result.Ok<ICommandDefinition>(sub);
        }

        return Result.Ok<ICommandDefinition>(commandDefinitions.First().rootCommand);
    }
}
