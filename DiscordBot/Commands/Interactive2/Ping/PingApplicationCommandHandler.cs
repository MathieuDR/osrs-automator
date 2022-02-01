using DiscordBot.Commands.Interactive2.Base.Handlers;

namespace DiscordBot.Commands.Interactive2.Ping;

public class PingApplicationCommandHandler : ApplicationCommandHandlerBase<PingCommandRequest> {
	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		await Context.RespondAsync($"At do work");
		return Result.Ok();
	}

	public PingApplicationCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) { }
}
