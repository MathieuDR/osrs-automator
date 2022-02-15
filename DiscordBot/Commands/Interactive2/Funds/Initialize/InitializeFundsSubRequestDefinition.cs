using DiscordBot.Commands.Interactive2.Base.Definitions;

namespace DiscordBot.Commands.Interactive2.Funds.Initialize; 

public class InitializeFundsSubRequestDefinition : SubCommandDefinitionBase<FundRootCommandDefinition> {
	public InitializeFundsSubRequestDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
	public override string Name => "configure";
	public override string Description => "Initializes or update the fund tracking.";

	public static string TrackingChannelOption => "tracking-channel";
	public static string DonationLeaderboardChannelOption => "donation-channel";
	public static string StartingFunds => "starting-funds";
	
	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		builder.AddOption(TrackingChannelOption, ApplicationCommandOptionType.Channel,"The channel to track funds in.", true, channelTypes: new List<ChannelType>()
			{
				ChannelType.Text
		});
		builder.AddOption(DonationLeaderboardChannelOption, ApplicationCommandOptionType.Channel,"The channel to track the donation leaderboard", true, channelTypes: new List<ChannelType>()
		{
			ChannelType.Text
		});
		
		builder.AddOption(StartingFunds, ApplicationCommandOptionType.String,"The starting funds, is 0 or the current funds", false);
		
		return Task.FromResult(builder);
	}
}