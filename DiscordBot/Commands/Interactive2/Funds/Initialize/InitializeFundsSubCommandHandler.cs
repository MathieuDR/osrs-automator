using DiscordBot.Commands.Interactive2.Base.Handlers;

namespace DiscordBot.Commands.Interactive2.Funds.Initialize;

public class InitializeFundsSubCommandHandler : ApplicationCommandHandlerBase<InitializeFundsSubCommandRequest> {
	public InitializeFundsSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) { }
	protected override Task<Result> DoWork(CancellationToken cancellationToken) => throw new NotImplementedException();
}