global using DiscordBot.Commands.Interactive2.Base.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Graveyard.Remove; 

public class RemoveShameSubCommandHandler : ApplicationCommandHandlerBase<RemoveShameSubCommandRequest> {
	private readonly IGraveyardService _graveyardService;
	public RemoveShameSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_graveyardService = serviceProvider.GetRequiredService<IGraveyardService>();
	}
	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var optionResult = GetOptions();

		if (optionResult.IsFailed) {
			return Result.Fail("Could not parse the parameters").WithErrors(optionResult.Errors);
		}

		var (user, id) = optionResult.Value;
		var result = await _graveyardService.RemoveShame(user.ToGuildUserDto(), id);

		_ = Context.CreateReplyBuilder(true)
			.FromResult(result)
			.RespondAsync();

		return Result.Ok();
		//return result;
	}

	private Result<(IUser user, Guid shameId)> GetOptions() {
		var user = Context.SubCommandOptions.GetOptionValue<IUser>(RemoveShameSubCommandDefinition.ShamedOption);
		var id = Context.SubCommandOptions.GetOptionValue<string>(RemoveShameSubCommandDefinition.ShameId);

		if (user is null) {
			return Result.Fail("User not found, is null.");
		}

		if (String.IsNullOrWhiteSpace(id)) {
			return Result.Fail("Id empty or null.");
		}
		
		if(!Guid.TryParse(id, out var parsedId)) {
			return Result.Fail("Id is not a valid Guid.");
		}

		return Result.Ok<(IUser user, Guid shameId)>((user, parsedId));
	}
}
