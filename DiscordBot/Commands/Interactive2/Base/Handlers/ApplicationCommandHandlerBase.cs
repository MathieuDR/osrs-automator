using DiscordBot.Commands.Interactive2.Base.Requests;

namespace DiscordBot.Commands.Interactive2.Base.Handlers;

public abstract class
	ApplicationCommandHandlerBase<TRequest> : CommandHandlerBase<TRequest, ApplicationCommandContext>
	where TRequest : ICommandRequest<ApplicationCommandContext> {
	protected ApplicationCommandHandlerBase(IServiceProvider serviceProvider) : base(serviceProvider) { }
    

}