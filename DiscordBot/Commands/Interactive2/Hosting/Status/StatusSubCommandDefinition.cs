namespace DiscordBot.Commands.Interactive2.Hosting.Status;

public class StatusSubCommandDefinition : SubCommandDefinitionBase<HostingRootCommandDefinition> {
	public StatusSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }

	public override string Name => "status";
	public override string Description => "Show hosting payment status";

	public static string ServerOption => "server";

	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		builder.AddOption(ServerOption, ApplicationCommandOptionType.String, "Server to inspect (omit for the full overview)", false,
			isAutocomplete: true);
		return Task.FromResult(builder);
	}
}
