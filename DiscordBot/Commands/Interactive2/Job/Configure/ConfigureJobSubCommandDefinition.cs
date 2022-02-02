using DiscordBot.Commands.Interactive2.Base.Definitions;
using DiscordBot.Common.Models.Enums;

namespace DiscordBot.Commands.Interactive2.Job.Configure; 

public class ConfigureJobSubCommandDefinition: SubCommandDefinitionBase<JobRootDefinition> {
	public ConfigureJobSubCommandDefinition(IServiceProvider serviceProvider) : base(serviceProvider) { }
	public override string Name => "configure";
	public override string Description => "Configure a job";
	
	public static string JobOption => "job";
	public static string ChannelOption => "channel";
	public static string EnabledOption => "enabled";
	
	protected override Task<SlashCommandOptionBuilder> ExtendOptionCommandBuilder(SlashCommandOptionBuilder builder) {
		
		builder.AddOption(new SlashCommandOptionBuilder()
				.WithName(JobOption)
				.WithDescription("The job to queue")
				.WithRequired(true)
				.AddEnumChoices( new List<JobType>(){JobType.GroupUpdate, JobType.MonthlyTop, JobType.MonthlyTopGains}))
			.AddOption(ChannelOption, ApplicationCommandOptionType.Channel, "The channel to send the message to", true)
			.AddOption(EnabledOption, ApplicationCommandOptionType.Boolean, "Whether the job is enabled", true);
		

		return Task.FromResult(builder);
	}
}