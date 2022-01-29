
using DiscordBot.Commands.Interactive2.Base.Handlers;
using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard.Shames;

public class ShamesCommandHandler : ApplicationCommandHandlerBase<ShamesCommandRequest> {
	private readonly IGraveyardService _graveyardService;
	private readonly MetricTypeParser _metricTypeParses;

	public ShamesCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_graveyardService = serviceProvider.GetRequiredService<IGraveyardService>();
		_metricTypeParses = serviceProvider.GetRequiredService<MetricTypeParser>();
	}

	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var (user, location, metricType) = GetOptions();
		var shames = await _graveyardService.GetShames(user.ToGuildUserDto(), location, metricType);

		if (shames.IsSuccess) {
			await PresentShames(shames.Value.ToArray(), user.DisplayName(), location, metricType);
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

	private async Task PresentShames(Shame[] shames, string user, ShameLocation? location, MetricType? metricType) {
		throw new NotImplementedException();
	}


	private (IUser, ShameLocation?, MetricType?) GetOptions() {
		var user = Context.SubCommandOptions.GetOptionValue<IUser>(ShamesSubCommandDefinition.ShamedOption) ?? Context.User;
		var locationRaw = Context.SubCommandOptions.GetOptionValue<string>(ShamesSubCommandDefinition.LocationOption);

		if (!String.IsNullOrWhiteSpace(locationRaw)) {
			var (location, locationMetric) = ShameExtensions.ToLocation(locationRaw, _metricTypeParses);
			return (user, location, locationMetric);
		}

		return (user, null, null);
	}
}
