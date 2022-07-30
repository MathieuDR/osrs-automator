namespace DiscordBot.Commands.Interactive2.Job; 

public class JobRootDefinition : RootCommandDefinitionBase{
	public JobRootDefinition(IServiceProvider serviceProvider, IEnumerable<ISubCommandDefinition> subCommandDefinitions) : base(serviceProvider, subCommandDefinitions) { }
	public override string Name => "job";
	public override string Description => "Job related commands";
	protected override Task<SlashCommandBuilder> ExtendBaseSlashCommandBuilder(SlashCommandBuilder builder) => Task.FromResult(builder);
}
