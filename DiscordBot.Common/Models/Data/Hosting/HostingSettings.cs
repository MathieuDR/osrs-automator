using DiscordBot.Common.Models.Data.Base;

namespace DiscordBot.Common.Models.Data.Hosting;

public record HostingSettings : BaseRecord {
	// Stored as long? to avoid nullable-struct mapper questions; convert to DiscordChannelId at the service boundary.
	public long? ReminderChannelId { get; init; }
}
