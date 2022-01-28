using DiscordBot.Commands.Interactive2.Base.Definitions;

namespace DiscordBot.Commands.Interactive2.Graveyard.Shames;

public class ShamesSubCommandDefinition : SubCommandDefinitionBase<GraveyardRootDefinition> {
	public ShamesSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
	public override string Name => "shames";
	public override string Description => "Shows the number of shames you or another player have!";
	public static string ShamedOption => "shamed";
	public static string LocationOption => "location";

	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		builder.AddOption(ShamedOption, ApplicationCommandOptionType.User, "Shame a user that died", false);
		builder.AddOption(LocationOption, ApplicationCommandOptionType.String, "Location of the shame", false);
		return Task.FromResult(builder);
	}

	protected override Task FillOptions() {
		var list = Options.ToList();
		list.Add((ShamedOption, typeof(string)));
		list.Add((LocationOption, typeof(string)));

		return base.FillOptions();
	}
}
