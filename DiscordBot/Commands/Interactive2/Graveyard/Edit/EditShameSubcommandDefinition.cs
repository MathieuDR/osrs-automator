global using DiscordBot.Commands.Interactive2.Base.Definitions;

namespace DiscordBot.Commands.Interactive2.Graveyard.Edit; 

public class EditShameSubcommandDefinition : SubCommandDefinitionBase<GraveyardRootDefinition>{
	public EditShameSubcommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
	public override string Name => "edit";
	public override string Description => "Edits the shame";

	public static string ShamedOption => "shamed";
	public static string ShameId => "id";
	public static string PictureOption => "picture";
	public static string LocationOption => "location";
	
	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		builder.AddOption(ShamedOption, ApplicationCommandOptionType.User, "Shame a user that died", true);
		builder.AddOption(ShameId, ApplicationCommandOptionType.String, "The ID of the shame to remove", true);
		builder.AddOption(LocationOption, ApplicationCommandOptionType.String, "Location of the shame", false, isAutocomplete: true);
		builder.AddOption(PictureOption, ApplicationCommandOptionType.String, "Picture url of the shame", false);
		return Task.FromResult(builder);
	}
}
