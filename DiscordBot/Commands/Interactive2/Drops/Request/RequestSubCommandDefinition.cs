namespace DiscordBot.Commands.Interactive2.Drops.Request; 

public class RequestSubCommandDefinition : SubCommandDefinitionBase<DropRootCommandDefinition> {
	public RequestSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
	public override string Name => "request";
	public override string Description => "Request an endpoint for the automated drops";
	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		return Task.FromResult(builder);
	}
}
