using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Base.Requests;

public abstract class
    CommandRequestBase<TCommandDefinition, TContext> : ICommandRequest<TCommandDefinition, TContext>
    where TContext : BaseInteractiveContext
    where TCommandDefinition : ICommandDefinition{
    public CommandRequestBase(TContext context) {
        Context = context;
    }

    public TContext Context { get; }
    public abstract AuthorizationRoles MinimumAuthorizationRole { get; }
}