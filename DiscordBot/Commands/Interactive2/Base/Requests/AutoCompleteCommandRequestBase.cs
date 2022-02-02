using DiscordBot.Commands.Interactive2.Base.Definitions;

namespace DiscordBot.Commands.Interactive2.Base.Requests;

public abstract class
	AutoCompleteCommandRequestBase<TCommandDefinition> :
		CommandRequestBase<TCommandDefinition, AutocompleteCommandContext>,
		IAutoCompleteCommandRequest<TCommandDefinition> where TCommandDefinition : ICommandDefinition {
	protected AutoCompleteCommandRequestBase(AutocompleteCommandContext context) : base(context) { }
}