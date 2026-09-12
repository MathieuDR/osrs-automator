using System.Collections.Concurrent;
using System.Globalization;
using DiscordBot.Common.Configuration;
using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data.Hosting;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Helpers;
using DiscordBot.Services.Interfaces;
using DiscordBot.Services.Models;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordBot.Services.Services;

/// <summary>
/// Singleton (unlike most services, which are transient): caches every <see cref="GuildHostingState"/>
/// so per-command footer reads don't hit the common LiteDB file each time. All writes go through
/// this instance, and the process is single-instance, so an in-memory cache is safe.
/// </summary>
public class HostingService : BaseService, IHostingService {
	private readonly IRepositoryStrategy _repositoryStrategy;
	private readonly MessageConfiguration _messages;
	private readonly BotTeamConfiguration _team;
	private readonly IClock _clock;
	private readonly Func<double> _randomSource;

	private readonly ConcurrentDictionary<DiscordGuildId, GuildHostingState> _cache = new();
	private readonly object _cacheLock = new();
	private volatile bool _cacheLoaded;

	private readonly object _settingsLock = new();
	private HostingSettings? _settingsCache;
	private volatile bool _settingsLoaded;

	public HostingService(ILogger<HostingService> logger, IRepositoryStrategy repositoryStrategy, MessageConfiguration messages,
		IOptions<BotTeamConfiguration> team, IClock clock, Func<double>? randomSource = null) : base(logger) {
		_repositoryStrategy = repositoryStrategy;
		_messages = messages;
		_team = team.Value;
		_clock = clock;
		_randomSource = randomSource ?? Random.Shared.NextDouble;
	}

	public HostingStatus GetStatus(DiscordGuildId guildId, string? guildName = null) {
		try {
			var state = TryGetState(guildId);
			var footerEnabled = state?.FooterEnabled ?? true;
			var lastPayment = LastPaymentOf(state);

			DateOnly? dueOn = null;
			int? daysOverdue = null;
			if (lastPayment is not null) {
				dueOn = HostingDates.ToDateOnly(lastPayment.PaidOn).AddMonths(lastPayment.TermMonths);
				daysOverdue = Today().DayNumber - dueOn.Value.DayNumber;
			}

			var footerText = BuildFooterText(guildId, guildName, footerEnabled, lastPayment, dueOn, daysOverdue);
			return new HostingStatus(guildId, footerEnabled, lastPayment, dueOn, daysOverdue, footerText);
		} catch (Exception ex) {
			Logger.LogWarning(ex, "Failed to get hosting status for guild {GuildId}", guildId);
			// Report the footer setting from whatever is already cached, rather than defaulting to
			// `false`: a read failure here must not be misreported as "footer off" for a guild that
			// actually has it on (or has never touched the setting, where the real default is `true`).
			var footerEnabled = _cache.TryGetValue(guildId, out var cached) ? cached.FooterEnabled : true;
			return new HostingStatus(guildId, footerEnabled, null, null, null, null);
		}
	}

	public Result<IReadOnlyList<HostingStatus>> GetOverview(IEnumerable<Guild> guilds) {
		try {
			IReadOnlyList<HostingStatus> statuses = guilds
				.Select(g => GetStatus(g.Id, g.Name))
				.OrderByDescending(s => s.DaysOverdue)
				.ToList();

			return Result.Ok(statuses);
		} catch (Exception ex) {
			Logger.LogWarning(ex, "Failed to build the hosting overview");
			return Result.Fail<IReadOnlyList<HostingStatus>>("Could not build the hosting overview");
		}
	}

	public Result RecordPayment(DiscordGuildId guildId, DateOnly paidOn, int termMonths, string? note, DiscordUserId by) {
		if (termMonths is < 1 or > 60) {
			return Result.Fail("termMonths must be between 1 and 60");
		}

		try {
			var state = GetOrCreateState(guildId);
			var payment = new HostingPayment {
				PaidOn = HostingDates.ToStorage(paidOn),
				TermMonths = termMonths,
				Note = note,
				RecordedBy = by
			};

			var updated = state with { Payments = state.Payments.Append(payment).ToList() };
			return Persist(updated);
		} catch (Exception ex) {
			Logger.LogWarning(ex, "Failed to record a hosting payment for guild {GuildId}", guildId);
			return Result.Fail("Could not record the payment");
		}
	}

	public Result SetFooterEnabled(DiscordGuildId guildId, bool enabled) {
		try {
			var state = GetOrCreateState(guildId);
			var updated = state with { FooterEnabled = enabled };
			return Persist(updated);
		} catch (Exception ex) {
			Logger.LogWarning(ex, "Failed to set footer enabled for guild {GuildId}", guildId);
			return Result.Fail("Could not update the footer setting");
		}
	}

	public Result SetReminderChannel(DiscordChannelId channel) {
		try {
			var current = LoadSettings();
			var updated = current with { ReminderChannelId = channel.Value };

			using var repo = _repositoryStrategy.GetOrCreateRepository<IHostingSettingsRepository>();
			var result = repo.UpdateOrInsert(updated);
			if (result.IsSuccess) {
				lock (_settingsLock) {
					_settingsCache = updated;
					_settingsLoaded = true;
				}
			}

			return result;
		} catch (Exception ex) {
			Logger.LogWarning(ex, "Failed to set the hosting reminder channel");
			return Result.Fail("Could not set the reminder channel");
		}
	}

	public Result<DiscordChannelId?> GetReminderChannel() {
		try {
			var settings = LoadSettings();
			DiscordChannelId? channel = settings.ReminderChannelId.HasValue ? new DiscordChannelId((ulong)settings.ReminderChannelId.Value) : null;
			return Result.Ok(channel);
		} catch (Exception ex) {
			Logger.LogWarning(ex, "Failed to load the hosting reminder channel");
			return Result.Fail<DiscordChannelId?>("Could not load the reminder channel");
		}
	}

	public Result MarkUpcomingReminderSent(DiscordGuildId guildId, DateOnly dueOn) {
		try {
			var state = GetOrCreateState(guildId);
			var updated = state with { UpcomingReminderSentForDueOn = HostingDates.ToStorage(dueOn) };
			return Persist(updated);
		} catch (Exception ex) {
			Logger.LogWarning(ex, "Failed to mark the upcoming reminder sent for guild {GuildId}", guildId);
			return Result.Fail("Could not mark the upcoming reminder sent");
		}
	}

	public Result MarkDueReminderSent(DiscordGuildId guildId, DateOnly dueOn) {
		try {
			var state = GetOrCreateState(guildId);
			var updated = state with { DueReminderSentForDueOn = HostingDates.ToStorage(dueOn) };
			return Persist(updated);
		} catch (Exception ex) {
			Logger.LogWarning(ex, "Failed to mark the due reminder sent for guild {GuildId}", guildId);
			return Result.Fail("Could not mark the due reminder sent");
		}
	}

	public bool ShouldDegrade(DiscordGuildId guildId, DiscordUserId userId) {
		try {
			var degraded = _messages.Hosting.Degraded;
			if (!degraded.Enabled) {
				return false;
			}

			if (guildId == _team.GuildId || userId == _team.OwnerId) {
				return false;
			}

			var state = TryGetState(guildId);
			var lastPayment = LastPaymentOf(state);
			if (state is null || lastPayment is null || !state.FooterEnabled) {
				return false;
			}

			var dueOn = HostingDates.ToDateOnly(lastPayment.PaidOn).AddMonths(lastPayment.TermMonths);
			var daysOverdue = Today().DayNumber - dueOn.DayNumber;
			if (daysOverdue < degraded.MinDaysOverdue) {
				return false;
			}

			return _randomSource() < degraded.FailureChance;
		} catch (Exception ex) {
			Logger.LogWarning(ex, "Failed to evaluate degraded mode for guild {GuildId}", guildId);
			return false;
		}
	}

	public string GetDegradedMessage(DiscordGuildId guildId, string? guildName = null) {
		try {
			IReadOnlyList<string> texts = _messages.Hosting.Degraded.Texts;
			if (texts is null || texts.Count == 0) {
				texts = HostingDefaults.Degraded;
			}

			var state = TryGetState(guildId);
			var lastPayment = LastPaymentOf(state);

			int days = 0;
			DateOnly? dueOn = null;
			if (lastPayment is not null) {
				dueOn = HostingDates.ToDateOnly(lastPayment.PaidOn).AddMonths(lastPayment.TermMonths);
				days = Math.Abs(Today().DayNumber - dueOn.Value.DayNumber);
			}

			var chosen = PickRandom(texts);
			return Substitute(chosen, guildId, guildName, days, dueOn, lastPayment?.PaidOn);
		} catch (Exception ex) {
			Logger.LogWarning(ex, "Failed to build the degraded message for guild {GuildId}", guildId);
			try {
				return Substitute(HostingDefaults.Degraded[0], guildId, guildName, 0, null, null);
			} catch (Exception ex2) {
				Logger.LogWarning(ex2, "Failed to build the fallback degraded message for guild {GuildId}", guildId);
				return "Command withheld: this server's hosting is unpaid. Retry.";
			}
		}
	}

	public IReadOnlyList<GuildHostingState> GetAllStates() {
		EnsureCacheLoaded();
		return _cache.Values.ToList();
	}

	private DateOnly Today() => HostingDates.ToDateOnly(_clock.UtcNow);

	private static HostingPayment? LastPaymentOf(GuildHostingState? state) =>
		state is { Payments.Count: > 0 } ? state.Payments[^1] : null;

	private string? BuildFooterText(DiscordGuildId guildId, string? guildName, bool footerEnabled, HostingPayment? lastPayment,
		DateOnly? dueOn, int? daysOverdue) {
		if (!footerEnabled) {
			return null;
		}

		if (lastPayment is null) {
			var neverPaid = _messages.Hosting.NeverPaid;
			if (neverPaid is null || neverPaid.Count == 0) {
				return null;
			}

			return Substitute(PickRandom(neverPaid), guildId, guildName, null, null, null);
		}

		if (daysOverdue is null || daysOverdue < 0) {
			return null;
		}

		IReadOnlyList<HostingTier> tiers = _messages.Hosting.Overdue;
		if (tiers is null || tiers.Count == 0) {
			tiers = HostingDefaults.Overdue;
		}

		var tier = tiers
			.Where(t => t.MinDays <= daysOverdue)
			.OrderByDescending(t => t.MinDays)
			.FirstOrDefault();

		if (tier?.Texts is null || tier.Texts.Count == 0) {
			return null;
		}

		return Substitute(PickRandom(tier.Texts), guildId, guildName, daysOverdue, dueOn, lastPayment.PaidOn);
	}

	private static string Substitute(string template, DiscordGuildId guildId, string? guildName, int? days, DateOnly? dueOn, DateTime? paidOn) {
		var result = template.Replace("{server}", guildName ?? guildId.ToString());

		result = result.Replace("{days}", days.HasValue ? Math.Abs(days.Value).ToString() : string.Empty);
		result = result.Replace("{date}", dueOn.HasValue ? dueOn.Value.ToString("d MMM yyyy", CultureInfo.InvariantCulture) : string.Empty);
		result = result.Replace("{paidDate}",
			paidOn.HasValue ? HostingDates.ToDateOnly(paidOn.Value).ToString("d MMM yyyy", CultureInfo.InvariantCulture) : string.Empty);

		return result;
	}

	private string PickRandom(IReadOnlyList<string> texts) {
		var index = (int)(_randomSource() * texts.Count);
		if (index < 0) {
			index = 0;
		} else if (index >= texts.Count) {
			index = texts.Count - 1;
		}

		return texts[index];
	}

	private void EnsureCacheLoaded() {
		if (_cacheLoaded) {
			return;
		}

		lock (_cacheLock) {
			if (_cacheLoaded) {
				return;
			}

			// A failed/thrown load must NOT set _cacheLoaded = true: that would permanently poison
			// the singleton cache as "loaded but empty" (every guild would look like "never paid"
			// until process restart). Leaving it false lets the next call retry.
			try {
				using var repo = _repositoryStrategy.GetOrCreateRepository<IGuildHostingStateRepository>();
				var all = repo.GetAll();
				if (!all.IsSuccess) {
					Logger.LogWarning("Failed to load hosting states from the repository, will retry on next access: {Errors}",
						string.Join("; ", all.Errors));
					return;
				}

				foreach (var state in all.Value) {
					_cache[state.GuildId] = state;
				}

				_cacheLoaded = true;
			} catch (Exception ex) {
				Logger.LogWarning(ex, "Failed to load hosting states from the repository, will retry on next access");
			}
		}
	}

	private GuildHostingState? TryGetState(DiscordGuildId guildId) {
		EnsureCacheLoaded();
		return _cache.TryGetValue(guildId, out var state) ? state : null;
	}

	private GuildHostingState GetOrCreateState(DiscordGuildId guildId) => TryGetState(guildId) ?? new GuildHostingState { GuildId = guildId };

	private Result Persist(GuildHostingState state) {
		using var repo = _repositoryStrategy.GetOrCreateRepository<IGuildHostingStateRepository>();
		var result = repo.UpdateOrInsert(state);
		if (result.IsSuccess) {
			_cache[state.GuildId] = state;
		}

		return result;
	}

	private HostingSettings LoadSettings() {
		if (_settingsLoaded) {
			return _settingsCache ?? new HostingSettings();
		}

		lock (_settingsLock) {
			if (_settingsLoaded) {
				return _settingsCache ?? new HostingSettings();
			}

			// Same rule as EnsureCacheLoaded: a failed/thrown load must NOT set _settingsLoaded =
			// true, so the next call retries instead of being stuck with "no reminder channel"
			// forever.
			try {
				using var repo = _repositoryStrategy.GetOrCreateRepository<IHostingSettingsRepository>();
				var result = repo.GetSingle();
				if (!result.IsSuccess) {
					Logger.LogWarning("Failed to load hosting settings from the repository, will retry on next access: {Errors}",
						string.Join("; ", result.Errors));
					return new HostingSettings();
				}

				_settingsCache = result.Value;
				_settingsLoaded = true;
				return _settingsCache ?? new HostingSettings();
			} catch (Exception ex) {
				Logger.LogWarning(ex, "Failed to load hosting settings from the repository, will retry on next access");
				return new HostingSettings();
			}
		}
	}
}
