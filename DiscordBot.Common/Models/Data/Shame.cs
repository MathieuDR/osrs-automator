using DiscordBot.Common.Models.Enums;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Common.Models.Data;

public record Shame {
	public Shame() {
		this.Id = Guid.NewGuid();
	}
	
	public Shame(ShameLocation location, MetricType? metricLocation, string imageUrl) : this() {
		Location = location;
		MetricLocation = metricLocation;
		ImageUrl = imageUrl;
	}
	
	public Guid Id { get; }
	public ShameLocation Location { get; init; }
	public MetricType? MetricLocation { get; init; }
	public string ImageUrl { get; init; }
}
