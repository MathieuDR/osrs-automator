namespace DiscordBot.Commands.Interactive2.Hosting;

public class HostingRootCommandDefinition : RootCommandDefinitionBase {
	public HostingRootCommandDefinition(IServiceProvider serviceProvider, IEnumerable<ISubCommandDefinition> subCommandDefinitions)
		: base(serviceProvider, subCommandDefinitions) { }

	public override string Name => "hosting";
	public override string Description => "Track which servers paid for hosting";

	protected override Task<SlashCommandBuilder> ExtendBaseSlashCommandBuilder(SlashCommandBuilder builder) {
		builder.WithDMPermission(false);
		return Task.FromResult(builder);
	}
}
