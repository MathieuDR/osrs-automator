using DiscordBot.Common.Models.Enums;
using LiteDB;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Common.Models.Data.Graveyard;

public record Shame {
	public Shame(ShameLocation location, MetricType? metricLocation, string imageUrl, DiscordUserId userId, DateTimeOffset? shamedAt = null) {
		Location = location;
		MetricLocation = metricLocation;
		ImageUrl = imageUrl;
		ShamedBy = userId;
		ShamedAt = shamedAt ?? DateTimeOffset.UtcNow;
	}

	[Obsolete("Only for LiteDB")]
	public Shame() {
		
	}

	public Guid Id { get; init; }
	public DateTimeOffset ShamedAt { get; init; }
	public ShameLocation Location { get; init; }
	public MetricType? MetricLocation { get; init; }
	public DiscordUserId ShamedBy { get; init; }
	public string ImageUrl { get; init; }

	[BsonIgnore]
	public string ShameLocationAsString => MetricLocation is not null ? MetricLocation.Value.ToDisplayNameOrFriendly() : Location.ToDisplayNameOrFriendly();
}
