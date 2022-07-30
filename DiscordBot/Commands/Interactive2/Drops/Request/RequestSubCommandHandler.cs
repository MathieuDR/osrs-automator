using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Drops.Request; 

public class RequestSubCommandHandler : ApplicationCommandHandlerBase<RequestSubCommandRequest> {
	private readonly IAutomatedDropperService _service;
	public RequestSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_service = serviceProvider.GetRequiredService<IAutomatedDropperService>();

	}
	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var user = Context.GuildUser.ToGuildUserDto();
		var urlResult = await _service.RequestUrl(user);

		_ = Context.CreateReplyBuilder(true).FromResult(urlResult)
			.WithSuccessEmbed<string>((builder, result) => {
				builder.WithDescription($"Your **Secret** url is: `{urlResult.Value}`")
					.AddField("**Note**", "Please do not share this url with anyone else.");
			})
			.RespondAsync();
		
		return Result.Ok();
	}
}
