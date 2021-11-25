using DiscordBot.Commands.Interactive2.Base.Definition;
using DiscordBot.Commands.Interactive2.Base.Requests;
using MediatR;

namespace DiscordBot.Commands.Interactive2.Base.Handlers;

public interface ICommandHandler<in TRequest, TContext> : IRequestHandler<TRequest, Result>
    where TRequest : ICommandRequest<TContext> where TContext : BaseInteractiveContext { }

public abstract class
    CommandHandlerBase<TRequest, TContext> : ICommandHandler<TRequest, TContext>
    where TRequest : ICommandRequest<TContext> where TContext : BaseInteractiveContext {
    public abstract Task<Result> Handle(TRequest request, CancellationToken cancellationToken);
}

public abstract class
    ApplicationCommandHandlerBase<TRequest> : ICommandHandler<TRequest, ApplicationCommandContext >
    where TRequest : ICommandRequest< ApplicationCommandContext> {
    public abstract Task<Result> Handle(TRequest request, CancellationToken cancellationToken);
}

public abstract class
    AutoCompleteHandlerBase<TRequest> : ICommandHandler<TRequest, AutocompleteCommandContext >
    where TRequest : ICommandRequest< AutocompleteCommandContext>  {
    public abstract Task<Result> Handle(TRequest request, CancellationToken cancellationToken);
}

public abstract class
    MessageComponentHandlerBase<TRequest> : ICommandHandler<TRequest, MessageComponentContext >
    where TRequest : ICommandRequest< MessageComponentContext>   {
    public abstract Task<Result> Handle(TRequest request, CancellationToken cancellationToken);
}
