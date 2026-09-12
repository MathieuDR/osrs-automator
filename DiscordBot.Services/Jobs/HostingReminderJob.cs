using System.Globalization;
using DiscordBot.Common.Configuration;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Data.Hosting;
using DiscordBot.Services.Helpers;
using DiscordBot.Services.Interfaces;
using DiscordBot.Services.Models;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordBot.Services.Jobs;

/// <summary>
/// Reminds the bot owner once ~30 days before a clan server's hosting is due, and once again on the
/// due date. See design doc §3.5: exactly two messages per due date (never more, never after the due
/// date one), until a new payment sets a new DueOn. <see cref="Decide"/> is a pure static function so
/// the decision table is testable without Quartz.
/// </summary>
public class HostingReminderJob : BaseJob {
	private readonly IHostingService _hosting;
	private readonly IDiscordService _discord;
	private readonly BotTeamConfiguration _team;

	public HostingReminderJob(ILogger<HostingReminderJob> logger, IHostingService hosting, IDiscordService discord,
		IOptions<BotTeamConfiguration> team) : base(logger) {
		_hosting = hosting;
		_discord = discord;
		_team = team.Value;
	}

	// BaseJob.DoWork is protected (invoked by Quartz through Execute); tests substitute
	// IHostingService/IDiscordService directly and need a way in without building a fake
	// IJobExecutionContext, hence this internal seam (DiscordBot.ServicesTests is a friend
	// assembly via InternalsVisibleTo).
	internal Task<Result> DoWorkForTests() => DoWork();

	protected override async Task<Result> DoWork() {
		var channelResult = _hosting.GetReminderChannel();
		if (channelResult.IsFailed) {
			Logger.LogWarning("Could not read hosting settings, skipping reminder run: {Errors}", string.Join("; ", channelResult.Errors));
			return Result.Ok();
		}

		if (channelResult.Value is not { } channel) {
			Logger.LogInformation("No hosting reminder channel configured, skipping reminder run");
			return Result.Ok();
		}

		var guildsResult = await _discord.GetGuilds();
		if (guildsResult.IsFailed) {
			return guildsResult.ToResult();
		}

		var states = _hosting.GetAllStates().ToDictionary(s => s.GuildId);
		var toRemind = new List<(DiscordGuildId GuildId, string Name, HostingStatus Status, ReminderKind Kind)>();

		foreach (var guild in guildsResult.Value) {
			if (!states.TryGetValue(guild.Id, out var state)) {
				continue;
			}

			var status = _hosting.GetStatus(guild.Id, guild.Name);
			if (status.DueOn is null) {
				continue;
			}

			var kind = Decide(status, state);
			if (kind != ReminderKind.None) {
				toRemind.Add((guild.Id, guild.Name, status, kind));
			}
		}

		if (toRemind.Count == 0) {
			return Result.Ok();
		}

		var isAlert = toRemind.Any(x => x.Kind == ReminderKind.Due);
		var fields = toRemind.Select(x => new EmbedFieldDto(x.Name, FormatField(x.Status))).ToArray();

		var sendResult = await _discord.SendMentionEmbed(channel, _team.OwnerId, "Hosting payments", fields, isAlert);
		if (sendResult.IsFailed) {
			return sendResult;
		}

		foreach (var (guildId, _, status, kind) in toRemind) {
			var dueOn = status.DueOn!.Value;
			if (kind == ReminderKind.Due) {
				_hosting.MarkDueReminderSent(guildId, dueOn);
			} else {
				_hosting.MarkUpcomingReminderSent(guildId, dueOn);
			}
		}

		return Result.Ok();
	}

	private static string FormatField(HostingStatus status) {
		var dueOn = status.DueOn!.Value;
		var daysOverdue = status.DaysOverdue!.Value;
		var daysText = daysOverdue >= 0 ? $"{daysOverdue} days overdue" : $"{-daysOverdue} days left";
		var text = $"due {dueOn.ToString("d MMM yyyy", CultureInfo.InvariantCulture)} ({daysText})";

		if (!string.IsNullOrWhiteSpace(status.LastPayment?.Note)) {
			text += $", {status.LastPayment!.Note}";
		}

		return text;
	}

	/// <summary>
	/// Pure decision table (design doc §3.5): no DueOn -> None; overdue (DaysOverdue >= 0) and the
	/// due reminder hasn't been sent for this exact DueOn -> Due; in the last 30 days before DueOn
	/// and the upcoming reminder hasn't been sent for this exact DueOn -> Upcoming; otherwise None.
	/// Due takes precedence: once overdue, an upcoming reminder that was never sent is skipped.
	/// "Sent for this DueOn" compares DateOnly values (via <see cref="HostingDates"/>), never raw
	/// DateTimes, since LiteDB round-trips stored UTC dates as Local-kind instants.
	/// </summary>
	public static ReminderKind Decide(HostingStatus status, GuildHostingState state) {
		if (status.DueOn is not { } dueOn || status.DaysOverdue is not { } daysOverdue) {
			return ReminderKind.None;
		}

		var dueSentForThisDueOn = state.DueReminderSentForDueOn is { } dueSent && HostingDates.ToDateOnly(dueSent) == dueOn;
		if (daysOverdue >= 0 && !dueSentForThisDueOn) {
			return ReminderKind.Due;
		}

		var upcomingSentForThisDueOn = state.UpcomingReminderSentForDueOn is { } upcomingSent && HostingDates.ToDateOnly(upcomingSent) == dueOn;
		if (daysOverdue is >= -30 and < 0 && !upcomingSentForThisDueOn) {
			return ReminderKind.Upcoming;
		}

		return ReminderKind.None;
	}
}
