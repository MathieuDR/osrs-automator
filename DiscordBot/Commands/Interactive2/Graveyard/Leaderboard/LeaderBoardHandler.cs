using DiscordBot.Commands.Interactive2.Base.Handlers;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data.Graveyard;
using DiscordBot.Common.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Helpers;
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

	private Task PresentLeaderboard((DiscordUserId user, Shame[] shames)[] memberShames, ShameLocation? location, MetricType? metricType) {
		var board = CreateLeaderboard(memberShames, location, metricType);

		_ = Context.CreatePaginatorReplyBuilder()
			.WithLeaderboard(board)
			.RespondAsync();

		return Task.CompletedTask;
	}

	private Models.DiscordLeaderBoard<int> CreateLeaderboard((DiscordUserId user, Shame[] shames)[] memberShames, ShameLocation? location, MetricType? metricType) {
		var board = new Models.DiscordLeaderBoard<int>() {
			Name =  location.HasValue ? metricType.HasValue ? metricType.Value.ToDisplayNameOrFriendly() : location.Value.ToDisplayNameOrFriendly() : "All" + " shame",
			ScoreFieldName = "Shames"
		};

		var shamesPerUser = memberShames.OrderByDescending(x => x.shames.Length).ToArray();
		for (var i = 0; i < shamesPerUser.Length; i++) {
			var (user, shames) = shamesPerUser[i];
			board.Entries.Add(new LeaderboardEntry<int>(Context.Guild.GetUser(user.UlongValue).DisplayName(), shames.Length, i + 1));
		}

		return board;
	}

	private (ShameLocation?, MetricType?) GetOptions() {
		var locationRaw = Context.SubCommandOptions.GetOptionValue<string>(LeaderboardSubCommandDefinition.LocationOption);

		if (String.IsNullOrWhiteSpace(locationRaw)) {
			return (null, null);
		}

		return ShameExtensions.ToLocation(locationRaw, _metricTypeParses);
	}
}
