namespace DiscordBot.Commands.Interactive2.Graveyard.OptIn;

public class OptInSubCommandDefinition : SubCommandDefinitionBase<GraveyardRootDefinition> {
	public OptInSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
	public override string Name => "enable";
	public override string Description => "Walk in the graveyard, be part of the graveyard.";
	
	public static string EnableOption => "enable";
	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		builder.AddOption(EnableOption, ApplicationCommandOptionType.Boolean, "Enable the graveyard.");
		return Task.FromResult(builder);
	}
}
