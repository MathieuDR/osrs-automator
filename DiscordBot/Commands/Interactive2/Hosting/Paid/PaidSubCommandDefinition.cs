namespace DiscordBot.Commands.Interactive2.Hosting.Paid;

public class PaidSubCommandDefinition : SubCommandDefinitionBase<HostingRootCommandDefinition> {
	public PaidSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }

	public override string Name => "paid";
	public override string Description => "Record a hosting payment for a server";

	public static string ServerOption => "server";
	public static string DateOption => "date";
	public static string MonthsOption => "months";
	public static string NoteOption => "note";

	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		builder.AddOption(ServerOption, ApplicationCommandOptionType.String, "The server that paid", true, isAutocomplete: true);
		builder.AddOption(DateOption, ApplicationCommandOptionType.String, "Date paid, yyyy-MM-dd (default today)", false);
		builder.AddOption(new SlashCommandOptionBuilder()
			.WithName(MonthsOption)
			.WithDescription("Term length in months (default 12)")
			.WithType(ApplicationCommandOptionType.Integer)
			.WithRequired(false)
			.WithMinValue(1)
			.WithMaxValue(60));
		builder.AddOption(NoteOption, ApplicationCommandOptionType.String, "Optional note", false);
		return Task.FromResult(builder);
	}
}
