using DiscordBot.Commands.Interactive2.Base.Handlers;
using DiscordBot.Common.Models.Enums;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard.Shame;

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
		
		var options = metricTypeOptions.Select(x => x.ToDisplayNameOrFriendly()).ToList();
		options.AddRange(locations.Select(x => x.ToDisplayNameOrFriendly()));

		return options.OrderBy(x=>x).ToArray();
	}
}
