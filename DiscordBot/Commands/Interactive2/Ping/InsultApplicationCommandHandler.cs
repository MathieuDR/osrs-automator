using DiscordBot.Commands.Interactive2.Base.Handlers;

namespace DiscordBot.Commands.Interactive2.Ping;

public class InsultApplicationCommandHandler : ApplicationCommandHandlerBase<InsultCommandRequest> {
	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		await Context.RespondAsync($"At insults do work");
		return Result.Ok();
	}

	public InsultApplicationCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) { }
}