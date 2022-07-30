namespace DiscordBot.Commands.Interactive2.Base.Handlers;

public abstract class
	AutoCompleteHandlerBase<TRequest> : CommandHandlerBase<TRequest, AutocompleteCommandContext>
	where TRequest : ICommandRequest<AutocompleteCommandContext> {
	protected AutoCompleteHandlerBase(IServiceProvider serviceProvider) : base(serviceProvider) { }
}