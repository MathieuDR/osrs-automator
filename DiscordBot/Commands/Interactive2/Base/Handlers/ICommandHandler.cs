using DiscordBot.Commands.Interactive2.Base.Definitions;
using DiscordBot.Commands.Interactive2.Base.Requests;
using MediatR;

namespace DiscordBot.Commands.Interactive2.Base.Handlers;

public interface ICommandHandler<in TRequest, TContext> : IRequestHandler<TRequest, Result>
    where TRequest : ICommandRequest<TContext> where TContext : BaseInteractiveContext { }

public abstract class
    CommandHandlerBase<TRequest, TContext> : ICommandHandler<TRequest, TContext>
    where TRequest : ICommandRequest<TContext> where TContext : BaseInteractiveContext {
    public Task<Result> Handle(TRequest request, CancellationToken cancellationToken) {
        // Set context and request
        Context = request.Context;
        Request = request;
        CommandDefinition = GetCommandDefinition();
        
        // Get options
        var options = GetOptions();
        
        // Do work
        return DoWork(options);
    }

    private ICommandDefinition GetCommandDefinition() {
        // Check if TRequest has generic parameter
        if (typeof(TRequest).IsGenericType) {
            // Get first generic parameter
            var genericType = typeof(TRequest).GetGenericArguments()[0];
            
            // Check if generic parameter is ICommandDefinition
            if (genericType.IsSubclassOf(typeof(ICommandDefinition))) {
                // Instantiate a object of the generic type
                return Activator.CreateInstance(genericType) as ICommandDefinition;

            }
        }

        throw new Exception("Cannot find generic parameter");
    }

    protected TContext Context { get; set; }
    protected TRequest Request { get; set; }
    protected ICommandDefinition CommandDefinition { get; set; }

    protected virtual IEnumerable<(string optionName, Type optionType, object? optionValue)> GetOptions() {
        var opts = CommandDefinition.Options;
        return opts.Select(x => (x.optionName, x.optionType, (object)null));
    }

    protected abstract Task<Result> DoWork(IEnumerable<(string optionName, Type optionType, object? optionValue)> options);
}

public abstract class
    ApplicationCommandHandlerBase<TRequest> : CommandHandlerBase<TRequest, ApplicationCommandContext>
    where TRequest : ICommandRequest<ApplicationCommandContext> {

}

public abstract class
    AutoCompleteHandlerBase<TRequest> :  CommandHandlerBase<TRequest, AutocompleteCommandContext >
    where TRequest : ICommandRequest< AutocompleteCommandContext>  {
  
}

public abstract class
    MessageComponentHandlerBase<TRequest> : CommandHandlerBase<TRequest, MessageComponentContext >
    where TRequest : ICommandRequest< MessageComponentContext>   {
}
