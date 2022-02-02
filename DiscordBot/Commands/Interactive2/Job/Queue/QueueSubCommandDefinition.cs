using DiscordBot.Commands.Interactive2.Base.Definitions;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Job.Queue; 

public class QueueSubCommandDefinition : SubCommandDefinitionBase<JobRootDefinition> {
	public QueueSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
	public override string Name => "queue";
	public override string Description => "Queue a job";
	
	public static string JobOption => "job";
	
	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		builder.AddOption(new SlashCommandOptionBuilder()
			.WithName(JobOption)
			.WithDescription("The job to queue")
			.WithRequired(true)
			.AddEnumChoices<JobType>());

		return Task.FromResult(builder);
	}
}