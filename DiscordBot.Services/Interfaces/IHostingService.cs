using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data.Hosting;
using DiscordBot.Services.Models;
using FluentResults;

namespace DiscordBot.Services.Interfaces;

public interface IHostingService {
	HostingStatus GetStatus(DiscordGuildId guildId, string? guildName = null); // never throws
	Result<IReadOnlyList<HostingStatus>> GetOverview(IEnumerable<Guild> guilds); // one per guild, sorted DaysOverdue desc (nulls last)
	Result RecordPayment(DiscordGuildId guildId, DateOnly paidOn, int termMonths, string? note, DiscordUserId by);
	Result SetFooterEnabled(DiscordGuildId guildId, bool enabled);
	Result SetReminderChannel(DiscordChannelId channel);
	Result<DiscordChannelId?> GetReminderChannel();
	Result MarkUpcomingReminderSent(DiscordGuildId guildId, DateOnly dueOn);
	Result MarkDueReminderSent(DiscordGuildId guildId, DateOnly dueOn);
	bool ShouldDegrade(DiscordGuildId guildId, DiscordUserId userId); // random draw inside
	string GetDegradedMessage(DiscordGuildId guildId, string? guildName = null);
	IReadOnlyList<GuildHostingState> GetAllStates(); // for the reminder job
	void RecordFooterShown(DiscordGuildId guildId, HostingStatus status, string? command); // no-op if status.FooterTemplate is null
	IReadOnlyList<TemplateCatalogEntry> GetTemplateCatalog();
}
