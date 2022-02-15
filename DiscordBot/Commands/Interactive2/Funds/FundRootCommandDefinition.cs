using DiscordBot.Commands.Interactive2.Base.Definitions;

namespace DiscordBot.Commands.Interactive2.Funds; 

public class FundRootCommandDefinition : RootCommandDefinitionBase {
	public FundRootCommandDefinition(IServiceProvider serviceProvider, IEnumerable<ISubCommandDefinition> subCommandDefinitions) : base(serviceProvider, subCommandDefinitions) { }
	public override string Name => "funds";
	public override string Description => "Manage your funds";
}
