using DiscordBot.Commands.Interactive2.Base.Handlers;
using DiscordBot.Common.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard.Leaderboard;

public class LeaderBoardHandler : ApplicationCommandHandlerBase<LeaderboardRequest> {
	private readonly IGraveyardService _graveyardService;
	private readonly MetricTypeParser _metricTypeParses;

	public LeaderBoardHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_graveyardService = serviceProvider.GetRequiredService<IGraveyardService>();
		_metricTypeParses = serviceProvider.GetRequiredService<MetricTypeParser>();
	}
	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var (location, metricType) = GetOptions();
		var shames = await _graveyardService.GetShames(Context.Guild.ToGuildDto(), location, metricType);
		
		if (shames.IsSuccess) {
			await PresentLeaderboard(shames.Value, location, metricType);
		} else {
			await Context.CreateReplyBuilder()
				.WithEmbed(x =>
					x.WithFailure("Something went wrong with getting the shames!")
						.AddField(f => {
							f.Name = "Message:";
							f.Value = shames.Errors.First().Message;
						})).RespondAsync();
		}

		return Result.Ok();
	}

	private async Task PresentLeaderboard((ulong user, Shame[] shames)[] toArray, ShameLocation? location, MetricType? metricType) {
		throw new NotImplementedException();
	}

	private (ShameLocation?, MetricType?) GetOptions() {
		var locationRaw = Context.SubCommandOptions.GetOptionValue<string>(LeaderboardSubCommandDefinition.LocationOption);

		if (String.IsNullOrWhiteSpace(locationRaw)) {
			return (null, null);
		}

		return ShameExtensions.ToLocation(locationRaw, _metricTypeParses);
	}
}
