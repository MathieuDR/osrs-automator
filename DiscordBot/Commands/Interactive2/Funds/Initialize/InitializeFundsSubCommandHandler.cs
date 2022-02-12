using DiscordBot.Commands.Interactive2.Base.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Funds.Initialize;

public class InitializeFundsSubCommandHandler : ApplicationCommandHandlerBase<InitializeFundsSubCommandRequest> {
	private readonly IClanFundsService _fundsService;

	public InitializeFundsSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_fundsService = serviceProvider.GetRequiredService<IClanFundsService>();

	}
	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var (trackingChannel, donationChannel, startingFunds, user) = GetOptions();
		var result = await _fundsService.Initialize(user.ToGuildUserDto(), trackingChannel.ToChannelDto(), donationChannel.ToChannelDto(), startingFunds);

		if (result.IsSuccess) {
			_ = Context.CreateReplyBuilder(true).WithEmbed(x=> x.WithSuccess("Funds Initialized!")).RespondAsync();
		}
		
		return Result.OkIf(result.IsSuccess, "Could not initialize funds")
			.WithErrors(result.Errors);
	}
	
	private (IChannel trackingChannel, IChannel donationChannel, long? startingFunds, IUser user) GetOptions() {
		var trackingChannel = Context.SubCommandOptions.GetOptionValue<IChannel>(InitializeFundsSubRequestDefinition.TrackingChannelOption);
		var donationChannel = Context.SubCommandOptions.GetOptionValue<IChannel>(InitializeFundsSubRequestDefinition.DonationLeaderboardChannelOption);
		var startingFundsAsString = Context.SubCommandOptions.GetOptionValue<string>(InitializeFundsSubRequestDefinition.StartingFunds);

		long? funds = string.IsNullOrWhiteSpace(startingFundsAsString) ? null : startingFundsAsString.ToLong();

		return (trackingChannel, donationChannel, funds, Context.User);
	}
}