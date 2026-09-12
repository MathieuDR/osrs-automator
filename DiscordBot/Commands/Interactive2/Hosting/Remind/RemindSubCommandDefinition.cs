namespace DiscordBot.Commands.Interactive2.Hosting.Remind;

public class RemindSubCommandDefinition : SubCommandDefinitionBase<HostingRootCommandDefinition> {
	public RemindSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }

	public override string Name => "remind";
	public override string Description => "Set the channel owner hosting reminders are posted to";

	public static string ChannelOption => "channel";

	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		builder.AddOption(ChannelOption, ApplicationCommandOptionType.Channel, "Channel to post reminders in", true);
		return Task.FromResult(builder);
	}
}
