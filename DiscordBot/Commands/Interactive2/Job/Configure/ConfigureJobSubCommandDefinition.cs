using DiscordBot.Commands.Interactive2.Base.Definitions;
using DiscordBot.Commands.Interactive2.Base.Handlers;
using DiscordBot.Commands.Interactive2.Base.Requests;
using DiscordBot.Common.Models.Enums;
using Microsoft.Extensions.DependencyInjection;

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

public class ConfigureJobSubCommandRequest : ApplicationCommandRequestBase<ConfigureJobSubCommandDefinition> {
	public ConfigureJobSubCommandRequest(ApplicationCommandContext context) : base(context) { }
	public override AuthorizationRoles MinimumAuthorizationRole => AuthorizationRoles.ClanAdmin;
}

public class ConfigureJobSubCommandHandler: ApplicationCommandHandlerBase<ConfigureJobSubCommandRequest> {
	private readonly IGroupService _groupService;

	public ConfigureJobSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_groupService = serviceProvider.GetRequiredService<IGroupService>();
	}
	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var jobType = Context.SubCommandOptions.GetOptionValueAsEnum<JobType>(ConfigureJobSubCommandDefinition.JobOption);
		var channel = Context.SubCommandOptions.GetOptionValue<IChannel>(ConfigureJobSubCommandDefinition.ChannelOption);
		var enabled = Context.SubCommandOptions.GetOptionValueOrDefault(ConfigureJobSubCommandDefinition.EnabledOption, true);
		
		await _groupService.SetAutomationJobChannel(jobType, Context.User.ToGuildUserDto(), channel.ToChannelDto(), enabled);

		_ = Context.CreateReplyBuilder()
			.WithEmbed(x => x.WithSuccess("Job configuration updated"))
			.RespondAsync();
		
		return Result.Ok();
	}
}
