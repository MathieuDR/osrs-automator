using DiscordBot.Commands.Interactive2.Base.Handlers;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBot.Commands.Interactive2.Graveyard.Leaderboard;

public class LeaderBoardHandler : ApplicationCommandHandlerBase<LeaderboardRequest> {
	private readonly IGraveyardService _graveyardService;
	private readonly MetricTypeParser _metricTypeParses;

	public LeaderBoardHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_graveyardService = serviceProvider.GetRequiredService<IGraveyardService>();
		_metricTypeParses = serviceProvider.GetRequiredService<MetricTypeParser>();
	}
	protected override Task<Result> DoWork(CancellationToken cancellationToken) {
		throw new NotImplementedException();
	}
}
