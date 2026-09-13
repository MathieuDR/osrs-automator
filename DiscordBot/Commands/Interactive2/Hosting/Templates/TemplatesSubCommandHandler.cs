using DiscordBot.Common.Configuration;
using DiscordBot.Services.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DiscordBot.Commands.Interactive2.Hosting.Templates;

public class TemplatesSubCommandHandler : ApplicationCommandHandlerBase<TemplatesSubCommandRequest> {
	private readonly IHostingService _hostingService;
	private readonly BotTeamConfiguration _team;
	private readonly DiscordSocketClient _client;

	public TemplatesSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_hostingService = serviceProvider.GetRequiredService<IHostingService>();
		_team = serviceProvider.GetRequiredService<IOptions<BotTeamConfiguration>>().Value;
		_client = serviceProvider.GetRequiredService<DiscordSocketClient>();
	}

	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var guardResult = HostingHandlerHelpers.EnsureOwnerGuild(Context, _team);
		if (guardResult.IsFailed) {
			return guardResult;
		}

		var serverValue = Context.SubCommandOptions.GetOptionValue<string>(TemplatesSubCommandDefinition.ServerOption);
		string? tierHeader = null;

		if (!string.IsNullOrWhiteSpace(serverValue)) {
			var serverResult = HostingHandlerHelpers.ResolveServer(serverValue, _client);
			if (serverResult.IsFailed) {
				return serverResult.ToResult();
			}

			var guildId = serverResult.Value;
			var name = _client.GetGuild(guildId.UlongValue)?.Name ?? serverValue;
			var status = _hostingService.GetStatus(guildId, name);
			tierHeader = $"{name} is currently on: {DescribeTier(status)}";
		}

		var catalog = _hostingService.GetTemplateCatalog();
		var lines = catalog
			.OrderBy(e => e.Kind == "NeverPaid" ? 0 : e.Kind == "Overdue" ? 1 : 2)
			.ThenBy(e => e.TierMinDays ?? -1)
			.Select(e => $"[{(e.Kind == "Overdue" ? $"Overdue {e.TierMinDays}+d" : e.Kind)}] {e.UsageCount}x — {e.Text}")
			.ToArray();

		if (lines.Length == 0) {
			lines = new[] { "No shame templates configured." };
		}

		var replyBuilder = Context.CreatePaginatorReplyBuilder(ephemeral: true)
			.WithLines(lines, 10, header: "Shame template catalog", inCodeBlock: true);

		if (tierHeader is not null) {
			replyBuilder = replyBuilder.WithFrontPage(new EmbedBuilder().WithSuccess(tierHeader));
		}

		await replyBuilder.RespondAsync();
		return Result.Ok();
	}

	private static string DescribeTier(HostingStatus status) {
		if (!status.FooterEnabled) {
			return "footer disabled for this server";
		}

		if (status.LastPayment is null) {
			return "never paid";
		}

		if (status.DaysOverdue is null || status.DaysOverdue < 0) {
			return "not currently overdue";
		}

		return status.FooterTierMinDays is { } tier ? $"tier {tier}+ days" : "overdue, but no tier configured";
	}
}
