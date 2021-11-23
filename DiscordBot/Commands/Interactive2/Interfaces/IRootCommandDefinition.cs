using DiscordBot.Common.Models.Enums;
using MediatR;

namespace DiscordBot.Commands.Interactive2.Interfaces;

public interface IHandleableCommand<out TCommandDefinition, out TContext, TContextType> : IRequest<Result>
    where TContext : BaseInteractiveContext<TContextType>
    where TContextType : SocketInteraction
    where TCommandDefinition : ICommandDefinition {
    public AuthorizationRoles MinimumAuthorizationRole { get; }
    public TContext Context { get; }

    public TCommandDefinition CommandDefinition { get; }
}

public interface ICommandDefinition {
    public string Name { get; }
    public string Description { get; }
}

public interface IRootCommandDefinition : ICommandDefinition {
    public Guid Id { get; }
    Task<uint> GetCommandBuilderHash();
    Task<SlashCommandProperties> GetCommandProperties();
}

public interface ISubCommandDefinition<TParentCommand> : ICommandDefinition
    where TParentCommand : IRootCommandDefinition {
    Task<SlashCommandOptionBuilder> GetOptionBuilder();
}

public interface ICommandHandler<in TCommand, TCommandDefinition, TContext, TContextType> : IRequestHandler<TCommand, Result>
    where TCommand : IHandleableCommand<TCommandDefinition, TContext, TContextType>
    where TContext : BaseInteractiveContext<TContextType>
    where TContextType : SocketInteraction
    where TCommandDefinition : ICommandDefinition { }
