namespace DiscordBot.Commands.Interactive2.Graveyard.Leaderboard;

public class LeaderboardSubCommandDefinition : SubCommandDefinitionBase<GraveyardRootDefinition> {
	public LeaderboardSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
	public override string Name => "leaderboard";
	public override string Description => "Shows the players ordered by shames!";
	public static string LocationOption => "location";

	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		builder.AddOption(LocationOption, ApplicationCommandOptionType.String, "Location of the shame", false, isAutocomplete: true);
		return Task.FromResult(builder);
	}
}