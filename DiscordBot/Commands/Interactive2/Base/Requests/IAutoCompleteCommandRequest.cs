namespace DiscordBot.Commands.Interactive2.Base.Requests;

public interface
	IAutoCompleteCommandRequest<out TCommandDefinition> : ICommandRequest<TCommandDefinition, AutocompleteCommandContext> where TCommandDefinition : ICommandDefinition { }