using DiscordBot.Commands.Interactive2.Base.Definitions;

namespace DiscordBot.Commands.Interactive2.Funds.Manage; 

public class ManageFundsSubCommandDefinition : SubCommandDefinitionBase<FundRootCommandDefinition> {
	public ManageFundsSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
	public override string Name => "manage";
	public override string Description => "Add, subtract funds";

	public static string AmountOption => "amount";
	public static string TypeOption => "type";
	public static string Player => "player";
	public static string Reason => "reason";
	
	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		builder.AddOption(Player, ApplicationCommandOptionType.User, "The player where the funds came / goes to", true);
		builder.AddOption(AmountOption, ApplicationCommandOptionType.User, "The amount of funds, can be negative", true);

		builder.AddOption(new SlashCommandOptionBuilder()
			.AddEnumChoices<ClanFundEventType>()
			.WithName(TypeOption)
			.WithDescription("Manage Type")
			.WithRequired(true));
		
		builder.AddOption(Reason, ApplicationCommandOptionType.User, "Notes", false);
		
		return Task.FromResult(builder);
	}
}