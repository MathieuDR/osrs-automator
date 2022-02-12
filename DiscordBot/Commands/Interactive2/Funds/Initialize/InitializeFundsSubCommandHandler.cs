using DiscordBot.Commands.Interactive2.Base.Handlers;

namespace DiscordBot.Commands.Interactive2.Funds.Initialize;

public class InitializeFundsSubCommandHandler : ApplicationCommandHandlerBase<InitializeFundsSubCommandRequest> {
	public InitializeFundsSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) { }
	protected override Task<Result> DoWork(CancellationToken cancellationToken) {
		var (trackingChannel, donationChannel, startingFunds, user) = GetOptions();
		
		
		return Task.FromResult<Result>(Result.Ok());
	}
	//
	// public static string TrackingChannelOption => "tracking-channel";
	// public static string DonationLeaderboardChannelOption => "donation-channel";
	// public static string StartingFunds => "starting-funds";
	
	private (IChannel trackingChannel, IChannel donationChannel, long? startingFunds, IUser user) GetOptions() {
		var trackingChannel = Context.SubCommandOptions.GetOptionValue<IChannel>(InitializeFundsSubRequestDefinition.TrackingChannelOption);
		var donationChannel = Context.SubCommandOptions.GetOptionValue<IChannel>(InitializeFundsSubRequestDefinition.DonationLeaderboardChannelOption);
		var startingFundsAsString = Context.SubCommandOptions.GetOptionValue<string>(InitializeFundsSubRequestDefinition.StartingFunds);


		return (trackingChannel, donationChannel, null, Context.User);
	}
}