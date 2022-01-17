using DiscordBot.Commands.Interactive2.Base.Definitions;

namespace DiscordBot.Commands.Interactive2.Graveyard.Leaderboard;

public class LeaderboardSubCommandDefinition : SubCommandDefinitionBase<GraveyardRootDefinition> {
	public LeaderboardSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
	public override string Name => "leaderboard";
	public override string Description => "Shows the top 10 players with the most shames!";

	private static string LocationOption => "location";

	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		builder.AddOption(LocationOption, ApplicationCommandOptionType.String, "Location of the shame", false);
		return Task.FromResult(builder);
	}

	protected override Task FillOptions() {
		var list = Options.ToList();
		list.Add((LocationOption, typeof(string)));
		return base.FillOptions();
	}
}
