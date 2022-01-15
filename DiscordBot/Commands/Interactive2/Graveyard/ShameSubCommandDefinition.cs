using DiscordBot.Commands.Interactive2.Base.Definitions;

namespace DiscordBot.Commands.Interactive2.Graveyard;

public class ShameSubCommandDefinition : SubCommandDefinitionBase<GraveyardRootDefinition> {
	public ShameSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
	public override string Name => "shame";
	public override string Description => "Shame somebody that died!";

	private static string ShamedOption => "shamed";
	private static string PictureOption => "picture";
	private static string LocationOption => "location";

	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		builder.AddOption(ShamedOption, ApplicationCommandOptionType.User, "Shame a user that died", true);
		builder.AddOption(LocationOption, ApplicationCommandOptionType.String, "Location of the shame", true);
		builder.AddOption(PictureOption, ApplicationCommandOptionType.String, "Picture url of the shame", false);
		return Task.FromResult(builder);
	}

	protected override Task FillOptions() {
		var list = Options.ToList();
		list.Add((ShamedOption, typeof(string)));
		list.Add((PictureOption, typeof(string)));
		list.Add((LocationOption, typeof(string)));

		return base.FillOptions();
	}
}