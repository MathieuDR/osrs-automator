using System.Globalization;
using DiscordBot.Common.Configuration;
using DiscordBot.Common.Identities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DiscordBot.Commands.Interactive2.Hosting.Paid;

public class PaidSubCommandHandler : ApplicationCommandHandlerBase<PaidSubCommandRequest> {
	private readonly IHostingService _hostingService;
	private readonly BotTeamConfiguration _team;
	private readonly DiscordSocketClient _client;

	public PaidSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_hostingService = serviceProvider.GetRequiredService<IHostingService>();
		_team = serviceProvider.GetRequiredService<IOptions<BotTeamConfiguration>>().Value;
		_client = serviceProvider.GetRequiredService<DiscordSocketClient>();
	}

	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var guardResult = HostingHandlerHelpers.EnsureOwnerGuild(Context, _team);
		if (guardResult.IsFailed) {
			return guardResult;
		}

		var serverValue = Context.SubCommandOptions.GetOptionValue<string>(PaidSubCommandDefinition.ServerOption);
		var serverResult = HostingHandlerHelpers.ResolveServer(serverValue, _client);
		if (serverResult.IsFailed) {
			return serverResult.ToResult();
		}

		var guildId = serverResult.Value;

		var dateString = Context.SubCommandOptions.GetOptionValue<string>(PaidSubCommandDefinition.DateOption);
		var paidOn = DateOnly.FromDateTime(DateTime.Today);
		if (!string.IsNullOrWhiteSpace(dateString)) {
			if (!DateOnly.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out paidOn)) {
				return Result.Fail($"Could not parse date '{dateString}', expected yyyy-MM-dd");
			}
		}

		var months = (int)Context.SubCommandOptions.GetOptionValueOrDefault<long>(PaidSubCommandDefinition.MonthsOption, 12);
		var note = Context.SubCommandOptions.GetOptionValue<string>(PaidSubCommandDefinition.NoteOption);

		var recordResult = _hostingService.RecordPayment(guildId, paidOn, months, note, Context.User.GetUserId());
		if (recordResult.IsFailed) {
			return recordResult;
		}

		var reminderChannelResult = _hostingService.GetReminderChannel();
		if (reminderChannelResult.IsSuccess && reminderChannelResult.Value is null) {
			_hostingService.SetReminderChannel(new DiscordChannelId(Context.Channel.Id));
		}

		var serverName = _client.GetGuild(guildId.UlongValue)?.Name ?? serverValue;
		var dueOn = paidOn.AddMonths(months);

		_ = Context.CreateReplyBuilder(ephemeral: true)
			.WithEmbed(x => x.WithSuccess($"{serverName}: paid {paidOn:d MMM yyyy}, {months} months, due {dueOn:d MMM yyyy}"))
			.RespondAsync();

		return Result.Ok();
	}
}
