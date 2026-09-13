using System.Text;
using DiscordBot.Common.Configuration;
using DiscordBot.Common.Models.Data.Hosting;
using DiscordBot.Services.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DiscordBot.Commands.Interactive2.Hosting.Status;

public class StatusSubCommandHandler : ApplicationCommandHandlerBase<StatusSubCommandRequest> {
	private readonly IHostingService _hostingService;
	private readonly IDiscordService _discordService;
	private readonly BotTeamConfiguration _team;
	private readonly DiscordSocketClient _client;

	public StatusSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_hostingService = serviceProvider.GetRequiredService<IHostingService>();
		_discordService = serviceProvider.GetRequiredService<IDiscordService>();
		_team = serviceProvider.GetRequiredService<IOptions<BotTeamConfiguration>>().Value;
		_client = serviceProvider.GetRequiredService<DiscordSocketClient>();
	}

	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var guardResult = HostingHandlerHelpers.EnsureOwnerGuild(Context, _team);
		if (guardResult.IsFailed) {
			return guardResult;
		}

		var serverValue = Context.SubCommandOptions.GetOptionValue<string>(StatusSubCommandDefinition.ServerOption);

		if (string.IsNullOrWhiteSpace(serverValue)) {
			return await RespondWithOverview();
		}

		return await RespondWithServerDetails(serverValue);
	}

	private async Task<Result> RespondWithOverview() {
		var guildsResult = await _discordService.GetGuilds();
		if (guildsResult.IsFailed) {
			return guildsResult.ToResult();
		}

		var guilds = guildsResult.Value.ToList();
		var overviewResult = _hostingService.GetOverview(guilds);
		if (overviewResult.IsFailed) {
			return overviewResult.ToResult();
		}

		var namesById = guilds.ToDictionary(g => g.Id, g => g.Name);
		var lines = overviewResult.Value
			.Select(s => HostingHandlerHelpers.FormatStatusLine(s, namesById.TryGetValue(s.GuildId, out var name) ? name : s.GuildId.ToString()))
			.ToArray();

		await Context.CreatePaginatorReplyBuilder(ephemeral: true)
			.WithLines(lines, 20, header: "Hosting status (most overdue first)", inCodeBlock: true)
			.RespondAsync();

		return Result.Ok();
	}

	private async Task<Result> RespondWithServerDetails(string serverValue) {
		var serverResult = HostingHandlerHelpers.ResolveServer(serverValue, _client);
		if (serverResult.IsFailed) {
			return serverResult.ToResult();
		}

		var guildId = serverResult.Value;
		var name = _client.GetGuild(guildId.UlongValue)?.Name ?? serverValue;
		var status = _hostingService.GetStatus(guildId, name);

		var state = _hostingService.GetAllStates().FirstOrDefault(x => x.GuildId == guildId);
		var history = state?.Payments.TakeLast(10).Reverse().ToList() ?? new List<HostingPayment>();

		var description = new StringBuilder();
		description.AppendLine(HostingHandlerHelpers.FormatStatusLine(status, name));

		if (status.FooterText is not null) {
			description.AppendLine();
			description.AppendLine($"Current footer: {status.FooterText}");
		}

		if (state is not null && state.DegradeDeniedCount + state.DegradeAllowedCount > 0) {
			var total = state.DegradeDeniedCount + state.DegradeAllowedCount;
			var percent = (double)state.DegradeDeniedCount / total * 100;
			description.AppendLine();
			description.AppendLine($"Degraded mode: {state.DegradeDeniedCount} denied / {state.DegradeAllowedCount} allowed ({percent:0.#}%) — excludes your own commands");
		}

		description.AppendLine();
		description.AppendLine("Payment history (most recent first):");
		if (history.Count == 0) {
			description.AppendLine("No payments recorded.");
		} else {
			foreach (var payment in history) {
				var paidOn = HostingDates.ToDateOnly(payment.PaidOn);
				var note = string.IsNullOrWhiteSpace(payment.Note) ? "" : $" ({payment.Note})";
				description.AppendLine($"{paidOn:d MMM yyyy} - {payment.TermMonths} months{note}");
			}
		}

		_ = Context.CreateReplyBuilder(ephemeral: true)
			.WithEmbed(x => x.WithSuccess(description.ToString(), name))
			.RespondAsync();

		return Result.Ok();
	}
}
