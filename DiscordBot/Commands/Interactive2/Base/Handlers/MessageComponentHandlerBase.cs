using DiscordBot.Commands.Interactive2.Base.Requests;

namespace DiscordBot.Commands.Interactive2.Base.Handlers;

public abstract class
	MessageComponentHandlerBase<TRequest> : CommandHandlerBase<TRequest, MessageComponentContext>
	where TRequest : ICommandRequest<MessageComponentContext> {
	protected MessageComponentHandlerBase(IServiceProvider serviceProvider) : base(serviceProvider) { }
}