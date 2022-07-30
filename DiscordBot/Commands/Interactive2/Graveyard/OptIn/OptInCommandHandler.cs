using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Graveyard.OptIn;

public class OptInCommandHandler: ApplicationCommandHandlerBase<OptInCommandRequest> {

	private IGraveyardService _graveyardService;
	public OptInCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_graveyardService = serviceProvider.GetRequiredService<IGraveyardService>();

	}

	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var user = Context.User;
		var enableOption = Context.SubCommandOptions[OptInSubCommandDefinition.EnableOption];

		var optingIn = (bool?)enableOption?.Value ?? true;

		Result result = null;
		var verb = optingIn ? "opted in to" : "opted out of";
		
		if (optingIn) {
			result = await _graveyardService.OptIn(user.ToGuildUserDto());
		} else {
			result = await _graveyardService.OptOut(user.ToGuildUserDto());
		}

		if (!result.IsSuccess) {
			return result;
		}

		await Context.CreateReplyBuilder()
			.WithEmbed(x=> x.WithSuccess($"Has {verb} the graveyard!", user.DisplayName()))
			.RespondAsync();

		return Result.Ok();
	}
}
