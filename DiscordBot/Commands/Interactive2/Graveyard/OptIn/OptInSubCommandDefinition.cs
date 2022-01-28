using DiscordBot.Commands.Interactive2.Base.Definitions;

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

	protected override Task FillOptions() {
		var opt = Options.ToList();
		opt.Add((EnableOption, typeof(bool))); 
		return base.FillOptions();
	}
}
