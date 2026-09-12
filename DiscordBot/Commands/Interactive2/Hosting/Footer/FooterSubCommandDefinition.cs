namespace DiscordBot.Commands.Interactive2.Hosting.Footer;

public class FooterSubCommandDefinition : SubCommandDefinitionBase<HostingRootCommandDefinition> {
	public FooterSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }

	public override string Name => "footer";
	public override string Description => "Enable or disable the hosting footer for a server";

	public static string ServerOption => "server";
	public static string EnabledOption => "enabled";

	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		builder.AddOption(ServerOption, ApplicationCommandOptionType.String, "The server to configure", true, isAutocomplete: true);
		builder.AddOption(EnabledOption, ApplicationCommandOptionType.Boolean, "Whether the footer is enabled", true);
		return Task.FromResult(builder);
	}
}
