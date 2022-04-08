namespace DiscordBot.Commands.Interactive2.MemberInfo; 

public class MemberInfoRootCommandDefinition : RootCommandDefinitionBase {
	public MemberInfoRootCommandDefinition(IServiceProvider serviceProvider, IEnumerable<ISubCommandDefinition> subCommandDefinitions) : base(serviceProvider, subCommandDefinitions) { }
	public override string Name => "memberinfo";
	public override string Description => "Gets information about members";
}