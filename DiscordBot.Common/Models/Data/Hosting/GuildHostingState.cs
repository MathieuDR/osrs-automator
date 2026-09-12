using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data.Base;

namespace DiscordBot.Common.Models.Data.Hosting;

public record GuildHostingState : BaseRecord {
	public DiscordGuildId GuildId { get; init; }
	public bool FooterEnabled { get; init; } = true;
	public List<HostingPayment> Payments { get; init; } = new();
	public DateTime? UpcomingReminderSentForDueOn { get; init; }
	public DateTime? DueReminderSentForDueOn { get; init; }
	public List<FooterHistoryEntry> FooterHistory { get; init; } = new();
	public int DegradeDeniedCount { get; init; }
	public int DegradeAllowedCount { get; init; }
}
