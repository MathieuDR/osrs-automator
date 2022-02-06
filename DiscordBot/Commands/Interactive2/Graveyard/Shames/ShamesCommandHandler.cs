
using System.Text;
using DiscordBot.Commands.Interactive2.Base.Handlers;
using DiscordBot.Common.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard.Shames;

public class ShamesCommandHandler : ApplicationCommandHandlerBase<ShamesCommandRequest> {
	private readonly IGraveyardService _graveyardService;
	private readonly MetricTypeParser _metricTypeParses;
	private readonly ILogger _logger;

	public ShamesCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		_graveyardService = serviceProvider.GetRequiredService<IGraveyardService>();
		_metricTypeParses = serviceProvider.GetRequiredService<MetricTypeParser>();
		_logger = serviceProvider.GetRequiredService<ILogger<ShamesCommandHandler>>();
	}

	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var (user, location, metricType) = GetOptions();
		var shames = await _graveyardService.GetShames(user.ToGuildUserDto(), location, metricType);

		if (shames.IsSuccess) {
			await PresentShames(shames.Value.ToArray(), user, location, metricType);
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

	private async Task PresentShames(Shame[] shames, IUser shamedUser, ShameLocation? location, MetricType? metricType) {
		var sb = new StringBuilder();
		sb.AppendLine("**Shames for " + shamedUser.Mention + "**");
		
		sb.Append($"Gravestones collected");
		
		if (location.HasValue) {
			sb.Append($" at ");
			sb.Append(metricType.HasValue ? metricType.Value.ToDisplayNameOrFriendly() : location.Value.ToDisplayNameOrFriendly());
		}
		
		sb.AppendLine($": {shames.Length}");
		
		// if shames is empty, send a no shame message
		if (shames.Length == 0) {
			await Context.CreateReplyBuilder().WithEmbedFrom("Squeaky clean!", sb.ToString()).RespondAsync();
			return;
		}

		var pages = shames.Select((s,i) => {
			_logger.LogInformation("{@shame}", s);
			return Context.CreatePageBuilder(Context.CreateEmbedBuilder().WithShame(s, i+1, shamedUser));
		});

		var paginator = Context.GetBaseStaticPaginatorBuilder(pages);
		//_ = Context.DeferAsync();
		_ = Context.SendPaginator(paginator.Build());
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
