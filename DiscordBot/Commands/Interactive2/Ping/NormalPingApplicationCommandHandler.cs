using DiscordBot.Commands.Interactive2.Base.Handlers;

namespace DiscordBot.Commands.Interactive2.Ping;

public class NormalPingApplicationCommandHandler : ApplicationCommandHandlerBase<NormalPingCommandRequest> {
	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		await Context.RespondAsync($"At normal do work");
		return Result.Ok();
	}

	public NormalPingApplicationCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) { }
}