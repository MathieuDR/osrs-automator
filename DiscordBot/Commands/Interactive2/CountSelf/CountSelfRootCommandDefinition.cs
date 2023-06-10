namespace DiscordBot.Commands.Interactive2.CountSelf; 

internal sealed class CountSelfRootCommandDefinition : RootCommandDefinitionBase {
    public CountSelfRootCommandDefinition(IServiceProvider serviceProvider, IEnumerable<ISubCommandDefinition> subCommandDefinitions) : base(serviceProvider, subCommandDefinitions) { }
    public override string Name => "count-self";
    public override string Description => "Give yourself points!";
}