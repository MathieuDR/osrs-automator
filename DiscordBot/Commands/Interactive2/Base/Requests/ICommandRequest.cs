using DiscordBot.Commands.Interactive2.Base.Definitions;
using DiscordBot.Common.Models.Enums;
using MediatR;

namespace DiscordBot.Commands.Interactive2.Base.Requests;
   

public interface ICommandRequest<out TContext>: IRequest<Result> 
    where TContext : BaseInteractiveContext{
    public AuthorizationRoles MinimumAuthorizationRole { get; }
    public TContext Context { get; }
}

public interface ICommandRequest<out TCommandDefinition, out TContext> : ICommandRequest<TContext>
    where TContext : BaseInteractiveContext
    where TCommandDefinition : ICommandDefinition {
}