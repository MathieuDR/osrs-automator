using DiscordBot.Commands.Interactive2.Base.Definitions;

namespace DiscordBot.Commands.Interactive2.Ping;

public class NormalSubCommandDefinition : SubCommandDefinitionBase<PingRootCommandDefinition> {
	public override string Name => "normal";
	public  override string Description => "Get a nice message";
    
	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		return Task.FromResult(builder);
	}

	public NormalSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
}