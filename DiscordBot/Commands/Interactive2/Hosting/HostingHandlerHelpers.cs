using DiscordBot.Common.Configuration;
using DiscordBot.Common.Identities;
using DiscordBot.Models.Contexts;
using DiscordBot.Services.Helpers;
using DiscordBot.Services.Models;

namespace DiscordBot.Commands.Interactive2.Hosting;

public static class HostingHandlerHelpers {
	/// <summary>
	/// Resolves a "server" autocomplete option value to a guild id: a raw ulong id first,
	/// then an exact (case-insensitive) name match against every guild the bot is in.
	/// </summary>
	public static Result<DiscordGuildId> ResolveServer(string value, DiscordSocketClient client) {
		if (string.IsNullOrWhiteSpace(value)) {
			return Result.Fail("Unknown server");
		}

		if (ulong.TryParse(value, out var id)) {
			return Result.Ok(new DiscordGuildId(id));
		}

		var guild = client.Guilds.FirstOrDefault(g => string.Equals(g.Name, value, StringComparison.OrdinalIgnoreCase));
		if (guild is not null) {
			return Result.Ok(guild.GetGuildId());
		}

		return Result.Fail("Unknown server");
	}

	public static Result EnsureOwnerGuild(ApplicationCommandContext ctx, BotTeamConfiguration team) {
		if (ctx.InnerContext.Channel is IGuildChannel && ctx.Guild.GetGuildId() == team.GuildId) {
			return Result.Ok();
		}

		return Result.Fail("Only available in the bot owner's server");
	}

	public static string FormatStatusLine(HostingStatus s, string name) {
		var paidDate = s.LastPayment is not null ? HostingDates.ToDateOnly(s.LastPayment.PaidOn).ToString("d MMM yyyy") : "never";
		var dueDate = s.DueOn is not null ? s.DueOn.Value.ToString("d MMM yyyy") : "-";

		string daysText;
		if (s.DaysOverdue is null) {
			daysText = "no record";
		} else if (s.DaysOverdue >= 0) {
			daysText = $"{s.DaysOverdue} overdue";
		} else {
			daysText = $"{-s.DaysOverdue.Value} days left";
		}

		var footerText = s.FooterEnabled ? "on" : "off";
		return $"{name,-24} paid {paidDate,-11} due {dueDate,-11} {daysText,-14} footer {footerText}";
	}
}
