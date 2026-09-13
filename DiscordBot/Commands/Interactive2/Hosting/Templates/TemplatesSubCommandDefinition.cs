namespace DiscordBot.Commands.Interactive2.Hosting.Templates;

public class TemplatesSubCommandDefinition : SubCommandDefinitionBase<HostingRootCommandDefinition> {
	public TemplatesSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }

	public override string Name => "templates";
	public override string Description => "List every configured shame text and how often it's been used";

	public static string ServerOption => "server";

	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		builder.AddOption(ServerOption, ApplicationCommandOptionType.String, "Show which tier this server is currently on (omit for just the catalog)", false,
			isAutocomplete: true);
		return Task.FromResult(builder);
	}
}
