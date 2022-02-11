using DiscordBot.Commands.Interactive2.Base.Requests;
using MediatR;

namespace DiscordBot.Commands.Interactive2.Base.Handlers;

public interface ICommandHandler<in TRequest, TContext> : IRequestHandler<TRequest, Result>
    where TRequest : ICommandRequest<TContext> where TContext : BaseInteractiveContext { }