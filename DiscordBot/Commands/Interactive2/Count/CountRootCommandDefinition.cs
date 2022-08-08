namespace DiscordBot.Commands.Interactive2.Count; 

public class CountRootCommandDefinition : RootCommandDefinitionBase {
    public CountRootCommandDefinition(IServiceProvider serviceProvider, IEnumerable<ISubCommandDefinition> subCommandDefinitions) : base(serviceProvider, subCommandDefinitions) { }
    public override string Name => "count";
    public override string Description => "Everything about point counting!";
}
