using DiscordBot.Common.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DiscordBot.Commands.Interactive2.Hosting.Remind;

public class RemindSubCommandHandler : ApplicationCommandHandlerBase<RemindSubCommandRequest> {
	private readonly IHostingService _hostingService;
	private readonly BotTeamConfiguration _team;

	public RemindSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_hostingService = serviceProvider.GetRequiredService<IHostingService>();
		_team = serviceProvider.GetRequiredService<IOptions<BotTeamConfiguration>>().Value;
	}

	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var guardResult = HostingHandlerHelpers.EnsureOwnerGuild(Context, _team);
		if (guardResult.IsFailed) {
			return guardResult;
		}

		var channel = Context.SubCommandOptions.GetOptionValue<IChannel>(RemindSubCommandDefinition.ChannelOption);
		var result = _hostingService.SetReminderChannel(channel.ToChannelDto().Id);
		if (result.IsFailed) {
			return result;
		}

		_ = Context.CreateReplyBuilder(ephemeral: true)
			.WithEmbed(x => x.WithSuccess($"Hosting reminders will be sent to {channel.Name}"))
			.RespondAsync();

		return Result.Ok();
	}
}
