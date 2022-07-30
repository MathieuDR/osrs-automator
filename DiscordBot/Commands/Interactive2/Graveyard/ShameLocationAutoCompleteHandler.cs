using DiscordBot.Common.Configuration;
using DiscordBot.Common.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Commands.Interactive2.Graveyard;

public class ShameLocationAutoCompleteHandler : AutoCompleteHandlerBase<ShameLocationAutoCompleteRequest> {
	public ShameLocationAutoCompleteHandler(IServiceProvider serviceProvider) : base(serviceProvider) {
		var synonymsConfiguration = serviceProvider.GetRequiredService<IOptions<MetricSynonymsConfiguration>>().Value;
		DefaultShameLocations = GetAllShameLocations(synonymsConfiguration);
	}
	protected override async Task<Result> DoWork(CancellationToken cancellationToken) {
		_ = Context.RespondAsync(GetShameLocationOptions(Context.CurrentOptionAsString));
		return Result.Ok();
	}

	private string[] GetShameLocationOptions(string start) {
		var lowered = start.ToLowerInvariant();
		return DefaultShameLocations.Where(x=> x.key.StartsWith(lowered)).Select(x=> x.value).Distinct().ToArray();
	}


	
	private (string key, string value)[] DefaultShameLocations { get; }

	private (string key, string value)[] GetAllShameLocations(MetricSynonymsConfiguration synonymsConfiguration) {
		var metricTypeOptions = MetricTypeCategory.Skills.GetMetricTypes();
		metricTypeOptions.AddRange(MetricTypeCategory.Bosses.GetMetricTypes());
		metricTypeOptions.AddRange(MetricTypeCategory.Activities.GetMetricTypes());

		var locations = new List<ShameLocation>() { ShameLocation.Wildy, ShameLocation.Other };
		
		var options = metricTypeOptions.Select(x => (x.ToDisplayNameOrFriendly().ToLowerInvariant(), x.ToDisplayNameOrFriendly())).ToList();
		options.AddRange(locations.Select(x => (x.ToDisplayNameOrFriendly().ToLowerInvariant(), x.ToDisplayNameOrFriendly())));


		foreach (var kvp in synonymsConfiguration.Synonyms) {
			foreach (var synonym in kvp.Value) {
				options.Add((synonym.ToLowerInvariant(), kvp.Key.ToDisplayNameOrFriendly()));
			}
		}

		return options.OrderBy(x=>x.Item2).ToArray();
	}
}
