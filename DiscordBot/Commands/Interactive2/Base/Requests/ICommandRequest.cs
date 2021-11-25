using DiscordBot.Commands.Interactive2.Base.Definition;
using DiscordBot.Common.Models.Enums;
using MediatR;

namespace DiscordBot.Commands.Interactive2.Base.Requests; 

public interface ICommandRequest<out TContext>: IRequest<Result>
    where TContext : BaseInteractiveContext{
    public AuthorizationRoles MinimumAuthorizationRole { get; }
}

public interface ICommandRequest<out TCommandDefinition, out TContext> : ICommandRequest<TContext>
    where TContext : BaseInteractiveContext
    where TCommandDefinition : ICommandDefinition {
}

public interface
    IApplicationCommandRequest<out TCommandDefinition> : ICommandRequest<TCommandDefinition, ApplicationCommandContext>
    where TCommandDefinition : ICommandDefinition { }

public interface
    IAutoCompleteCommandRequest<out TCommandDefinition> : ICommandRequest<TCommandDefinition, AutocompleteCommandContext> where TCommandDefinition : ICommandDefinition { }

public interface
    IMessageComponentCommandRequest<out TCommandDefinition> : ICommandRequest<TCommandDefinition, MessageComponentContext>
    where TCommandDefinition : ICommandDefinition { }

