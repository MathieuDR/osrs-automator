using DiscordBot.Commands.Interactive2.Base.Definitions;

namespace DiscordBot.Commands.Interactive2.Graveyard.Remove; 

public class RemoveShameSubCommandDefinition : SubCommandDefinitionBase<GraveyardRootDefinition>{
	public RemoveShameSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
	public override string Name => "remove";
	public override string Description => "Removes a shame from the graveyard";
	
	public static string ShamedOption => "shamed";
	public static string ShameId => "id";
	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		builder.AddOption(ShamedOption, ApplicationCommandOptionType.User, "Shame a user that died", true);
		builder.AddOption(ShameId, ApplicationCommandOptionType.String, "The ID of the shame to remove", true);
		return Task.FromResult(builder);
	}
}
