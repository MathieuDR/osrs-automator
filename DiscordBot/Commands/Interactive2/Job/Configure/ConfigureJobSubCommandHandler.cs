using DiscordBot.Commands.Interactive2.Base.Handlers;
using DiscordBot.Common.Models.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Job.Configure;

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