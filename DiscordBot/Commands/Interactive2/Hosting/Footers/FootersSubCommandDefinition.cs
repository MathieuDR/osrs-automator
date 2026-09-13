namespace DiscordBot.Commands.Interactive2.Hosting.Footers;

public class FootersSubCommandDefinition : SubCommandDefinitionBase<HostingRootCommandDefinition> {
	public FootersSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }

	public override string Name => "footers";
	public override string Description => "Show the footer nag history logged for a server";

	public static string ServerOption => "server";

	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		builder.AddOption(ServerOption, ApplicationCommandOptionType.String, "Server to inspect", true, isAutocomplete: true);
		return Task.FromResult(builder);
	}
}
