using DiscordBot.Common.Models.Enums;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Common.Models.Data;

public record Shame {
	public Shame() {
		Id = Guid.NewGuid();
	}
	
	public Shame(ShameLocation location, MetricType? metricLocation, string imageUrl, ulong userId, DateTimeOffset? shamedAt = null) : this() {
		Location = location;
		MetricLocation = metricLocation;
		ImageUrl = imageUrl;
		ShamedBy = userId;
		ShamedAt = shamedAt ?? DateTimeOffset.UtcNow;
	}
	
	
	public Guid Id { get; }
	public DateTimeOffset ShamedAt { get; init; }
	public ShameLocation Location { get; init; }
	public MetricType? MetricLocation { get; init; }
	public ulong ShamedBy { get; init; }
	public string ImageUrl { get; init; }

	public string ShameLocationAsString => MetricLocation is not null ? MetricLocation.Value.ToDisplayNameOrFriendly() : Location.ToDisplayNameOrFriendly();
}
