namespace DiscordBot.Commands.Interactive2.Ping;

public class InsultSubCommandDefinition : SubCommandDefinitionBase<PingRootCommandDefinition> {
	public override string Name => "insult";
	public  override string Description => "Receive an insult";
	public string VariantOption => "variant";
    
	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		builder.AddOption(VariantOption, ApplicationCommandOptionType.Boolean,"Variant of insult", false);
		return Task.FromResult(builder);
	}

	public InsultSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
}