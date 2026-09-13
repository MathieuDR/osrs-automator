using System.Globalization;
using DiscordBot.Common.Configuration;
using DiscordBot.Common.Models.Data.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DiscordBot.Commands.Interactive2.Hosting.Footers;

public class FootersSubCommandHandler : ApplicationCommandHandlerBase<FootersSubCommandRequest> {
	private readonly IHostingService _hostingService;
	private readonly BotTeamConfiguration _team;
	private readonly DiscordSocketClient _client;

	public FootersSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_hostingService = serviceProvider.GetRequiredService<IHostingService>();
		_team = serviceProvider.GetRequiredService<IOptions<BotTeamConfiguration>>().Value;
		_client = serviceProvider.GetRequiredService<DiscordSocketClient>();
	}

	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var guardResult = HostingHandlerHelpers.EnsureOwnerGuild(Context, _team);
		if (guardResult.IsFailed) {
			return guardResult;
		}

		var serverValue = Context.SubCommandOptions.GetOptionValue<string>(FootersSubCommandDefinition.ServerOption);
		var serverResult = HostingHandlerHelpers.ResolveServer(serverValue, _client);
		if (serverResult.IsFailed) {
			return serverResult.ToResult();
		}

		var guildId = serverResult.Value;
		var name = _client.GetGuild(guildId.UlongValue)?.Name ?? serverValue;

		var state = _hostingService.GetAllStates().FirstOrDefault(x => x.GuildId == guildId);
		var history = state?.FooterHistory.AsEnumerable().Reverse().ToList() ?? new List<FooterHistoryEntry>();

		var lines = history.Count == 0
			? new[] { "No footers logged for this server yet." }
			: history.Select(e => $"{e.ShownAt.ToString("d MMM yyyy HH:mm", CultureInfo.InvariantCulture)} [{e.Command ?? "-"}] {e.DisplayText}").ToArray();

		await Context.CreatePaginatorReplyBuilder(ephemeral: true)
			.WithLines(lines, 10, header: $"Footer history for {name} (newest first)", inCodeBlock: true)
			.RespondAsync();

		return Result.Ok();
	}
}
