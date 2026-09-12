using DiscordBot.Common.Identities;

namespace DiscordBot.Common.Models.Data.Hosting;

public record HostingPayment {
	public DateTime PaidOn { get; init; }
	public int TermMonths { get; init; } = 12;
	public string? Note { get; init; }
	public DiscordUserId RecordedBy { get; init; }
}
