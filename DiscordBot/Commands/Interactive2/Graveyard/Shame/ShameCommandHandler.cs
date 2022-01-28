using System.Runtime;
using DiscordBot.Commands.Interactive2.Base.Handlers;
using DiscordBot.Common.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard.Shame;

public class ShameCommandHandler : ApplicationCommandHandlerBase<ShameCommandRequest> {
	private readonly IGraveyardService _graveyardService;

	public ShameCommandHandler(IServiceProvider serviceProvider) : base(serviceProvider) =>
		_graveyardService = serviceProvider.GetRequiredService<IGraveyardService>();

	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		var shamedUserString = Context.SubCommandOptions.GetOptionValue<string>(ShameSubCommandDefinition.ShamedOption);
		var locationString = Context.SubCommandOptions.GetOptionValue<string>(ShameSubCommandDefinition.LocationOption);
		var pictureUrl = Context.SubCommandOptions.GetOptionValue<string>(ShameSubCommandDefinition.PictureOption);

		var users = (await shamedUserString.GetUsersFromString(Context)).users.ToList();


		return Result.Ok();
	}
}

public class ShameAutoCompleteHandler : AutoCompleteHandlerBase<ShameAutoCompleteRequest> {
	public ShameAutoCompleteHandler(IServiceProvider serviceProvider) : base(serviceProvider) { }
	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		_ = Context.RespondAsync(GetShameLocationOptions(Context.CurrentOptionAsString));
			return Result.Ok();
	}

	private string[] GetShameLocationOptions(string start) {
		var lowered = start.ToLowerInvariant();
		return DefaultShameLocations.Where(x=> x.ToLowerInvariant().StartsWith(lowered)).ToArray();
	}

	private string[] _defaultShameLocations = null;
	private string[] DefaultShameLocations => _defaultShameLocations ??= GetAllShameLocations();

	private string[] GetAllShameLocations() {
		var metricTypeOptions = MetricTypeCategory.Skills.GetMetricTypes();
		metricTypeOptions.AddRange(MetricTypeCategory.Bosses.GetMetricTypes());
		metricTypeOptions.AddRange(MetricTypeCategory.Activities.GetMetricTypes());

		var locations = new List<ShameLocation>() { ShameLocation.Wildy, ShameLocation.Other };
		
		var options = metricTypeOptions.Select(x => x.ToFriendlyNameOrDefault()).ToList();
		options.AddRange(locations.Select(x => x.ToFriendlyNameOrDefault()));

		return options.OrderBy(x=>x).ToArray();
	}
}
