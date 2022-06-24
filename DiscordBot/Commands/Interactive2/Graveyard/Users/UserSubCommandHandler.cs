using DiscordBot.Common.Identities;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Graveyard.Users;

public class UserSubCommandHandler : ApplicationCommandHandlerBase<UserSubCommandRequest> {
	private readonly IGraveyardService _graveyardService;

	public UserSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_graveyardService = serviceProvider.GetRequiredService<IGraveyardService>();
	}
	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var users = await _graveyardService.GetOptedInUsers(Context.Guild.ToGuildDto());

		if (users.IsFailed) {
			return users.ToResult();
		}
		
		// Create a string builder to build the message in pages
		await PresentUsers(users.Value);
		
		return Result.Ok();
	}

	private Task PresentUsers(DiscordUserId[] usersValue) {
		var strings = usersValue.Select(x=> $"<@{x}>").ToArray();
		// Send the message
		_ = Context.CreatePaginatorReplyBuilder().WithLines(strings, modifications: x => x.WithTitle("Opted in to the graveyard")).RespondAsync();
		
		return Task.CompletedTask;
	}
}