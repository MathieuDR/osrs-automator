using DiscordBot.Commands.Interactive2.Base.Definitions;

namespace DiscordBot.Commands.Interactive2.Graveyard;

public class GraveyardRootDefinition : RootCommandDefinitionBase {
	public GraveyardRootDefinition(IServiceProvider serviceProvider, IEnumerable<ISubCommandDefinition> subCommandDefinitions) : base(serviceProvider,
		subCommandDefinitions) { }

	public override string Name => "graveyard";
	public override string Description => "Graveyard of shame commands";

	protected override Task<SlashCommandBuilder> ExtendBaseSlashCommandBuilder(SlashCommandBuilder builder) => Task.FromResult(builder);
}