global using DiscordBot.Commands.Interactive2.Base.Definitions;

namespace DiscordBot.Commands.Interactive2.Drops; 

public class DropRootCommandDefinition : RootCommandDefinitionBase {
	public DropRootCommandDefinition(IServiceProvider serviceProvider, IEnumerable<ISubCommandDefinition> subCommandDefinitions) : base(serviceProvider, subCommandDefinitions) { }
	public override string Name => "drops";
	public override string Description => "Commands for managing drops.";
}
