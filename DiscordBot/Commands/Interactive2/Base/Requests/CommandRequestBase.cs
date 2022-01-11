using DiscordBot.Commands.Interactive2.Base.Definitions;
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

public abstract class
    ApplicationCommandRequestBase<TCommandDefinition> : CommandRequestBase<TCommandDefinition, ApplicationCommandContext>,
        IApplicationCommandRequest<TCommandDefinition> where TCommandDefinition : ICommandDefinition{
    protected ApplicationCommandRequestBase(ApplicationCommandContext context) : base(context) { }
}

public abstract class
    AutoCompleteCommandRequestBase<TCommandDefinition> :
        CommandRequestBase<TCommandDefinition, AutocompleteCommandContext>,
        IAutoCompleteCommandRequest<TCommandDefinition> where TCommandDefinition : ICommandDefinition {
    protected AutoCompleteCommandRequestBase(AutocompleteCommandContext context) : base(context) { }
}

public abstract class
    MessageComponentContextBase<TCommandDefinition> : CommandRequestBase<TCommandDefinition, MessageComponentContext>,
        IMessageComponentCommandRequest<TCommandDefinition> where TCommandDefinition : ICommandDefinition {
    protected MessageComponentContextBase(MessageComponentContext context) : base(context) { }
}
