using MediatR;

namespace DiscordBot.Commands.Interactive2.Confirm; 

internal sealed class ConfirmRootCommandDefinition : RootCommandDefinitionBase{
    public ConfirmRootCommandDefinition(IServiceProvider serviceProvider, IEnumerable<ISubCommandDefinition> subCommandDefinitions) : base(serviceProvider, subCommandDefinitions) { }
    public override string Name => "confirm";
    public override string Description => "Confirm button stuff";
}