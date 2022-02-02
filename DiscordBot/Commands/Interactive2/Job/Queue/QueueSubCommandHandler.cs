using DiscordBot.Commands.Interactive2.Base.Handlers;
using DiscordBot.Common.Models.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Job.Queue;

public class QueueSubCommandHandler : ApplicationCommandHandlerBase<QueueSubCommandRequest> {
	private readonly IGroupService _groupService;

	public QueueSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_groupService = serviceProvider.GetRequiredService<IGroupService>();
	}
	protected override Task<Result> DoWork(CancellationToken cancellationToken) {
		var jobType = Context.SubCommandOptions.GetOptionValueAsEnum<JobType>(QueueSubCommandDefinition.JobOption);
		_groupService.QueueJob(jobType);

		_ = Context
			.CreateReplyBuilder(true)
			.WithEmbed(e => e
				.WithSuccess($"queued job {jobType}"))
			.RespondAsync();

		return Task.FromResult(Result.Ok());
	}
}