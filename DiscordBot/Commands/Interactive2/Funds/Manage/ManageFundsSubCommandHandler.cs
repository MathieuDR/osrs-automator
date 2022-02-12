using DiscordBot.Commands.Interactive2.Base.Handlers;

namespace DiscordBot.Commands.Interactive2.Funds.Manage;

public class ManageFundsSubCommandHandler: ApplicationCommandHandlerBase<ManageFundsSubCommandRequest> {
	public ManageFundsSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) { }
	protected override Task<Result> DoWork(CancellationToken cancellationToken) => throw new NotImplementedException();
}