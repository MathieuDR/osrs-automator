using DiscordBot.Common.Models.Enums;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Common.Models.Data;

public record Shame {
	public Shame() {
		this.Id = Guid.NewGuid();
		ShamedAt ??= DateTimeOffset.UtcNow;
	}
	
	public Shame(ShameLocation location, MetricType? metricLocation, string imageUrl, ulong userId) : this() {
		Location = location;
		MetricLocation = metricLocation;
		ImageUrl = imageUrl;
		ShamedBy = userId;
	}
	
	public Shame(ShameLocation location, MetricType? metricLocation, string imageUrl, ulong userId, DateTimeOffset shamedAt) : 
		this(location, metricLocation, imageUrl, userId) {
		ShamedAt = shamedAt;
	}
	
	public Guid Id { get; }
	public DateTimeOffset? ShamedAt { get; init; }
	public ShameLocation Location { get; init; }
	public MetricType? MetricLocation { get; init; }
	public ulong ShamedBy { get; init; }
	public string ImageUrl { get; init; }
}
