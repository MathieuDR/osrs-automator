using DiscordBot.Common.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DiscordBot.Commands.Interactive2.Hosting.Footer;

public class FooterSubCommandHandler : ApplicationCommandHandlerBase<FooterSubCommandRequest> {
	private readonly IHostingService _hostingService;
	private readonly BotTeamConfiguration _team;
	private readonly DiscordSocketClient _client;

	public FooterSubCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_hostingService = serviceProvider.GetRequiredService<IHostingService>();
		_team = serviceProvider.GetRequiredService<IOptions<BotTeamConfiguration>>().Value;
		_client = serviceProvider.GetRequiredService<DiscordSocketClient>();
	}

	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var guardResult = HostingHandlerHelpers.EnsureOwnerGuild(Context, _team);
		if (guardResult.IsFailed) {
			return guardResult;
		}

		var serverValue = Context.SubCommandOptions.GetOptionValue<string>(FooterSubCommandDefinition.ServerOption);
		var serverResult = HostingHandlerHelpers.ResolveServer(serverValue, _client);
		if (serverResult.IsFailed) {
			return serverResult.ToResult();
		}

		var enabled = Context.SubCommandOptions.GetOptionValue<bool>(FooterSubCommandDefinition.EnabledOption);

		var result = _hostingService.SetFooterEnabled(serverResult.Value, enabled);
		if (result.IsFailed) {
			return result;
		}

		var name = _client.GetGuild(serverResult.Value.UlongValue)?.Name ?? serverValue;

		_ = Context.CreateReplyBuilder(ephemeral: true)
			.WithEmbed(x => x.WithSuccess($"{name}: footer {(enabled ? "enabled" : "disabled")}"))
			.RespondAsync();

		return Result.Ok();
	}
}
