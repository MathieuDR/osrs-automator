using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data.Hosting;

namespace DiscordBot.Services.Models;

public record HostingStatus(DiscordGuildId GuildId, bool FooterEnabled, HostingPayment? LastPayment,
	DateOnly? DueOn, int? DaysOverdue, string? FooterText, string? FooterTemplate, int? FooterTierMinDays);
