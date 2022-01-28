using System.Runtime;
using DiscordBot.Commands.Interactive2.Base.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Graveyard.Shame;

public class ShameCommandHandler : ApplicationCommandHandlerBase<ShameCommandRequest> {
	private readonly IGraveyardService _graveyardService;

	public ShameCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) =>
		_graveyardService = serviceProvider.GetRequiredService<IGraveyardService>();

	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var shamedUserString = Context.SubCommandOptions.GetOptionValue<string>(ShameSubCommandDefinition.ShamedOption);
		var locationString = Context.SubCommandOptions.GetOptionValue<string>(ShameSubCommandDefinition.LocationOption);
		var pictureUrl = Context.SubCommandOptions.GetOptionValue<string>(ShameSubCommandDefinition.PictureOption);

		var users = (await shamedUserString.GetUsersFromString(Context)).users.ToList();


		return Result.Ok();
	}
}