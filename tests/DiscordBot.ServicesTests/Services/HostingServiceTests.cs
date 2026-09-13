using DiscordBot.Common.Configuration;
using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data.Hosting;
using DiscordBot.Data;
using DiscordBot.Data.Configuration;
using DiscordBot.Data.Factories;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository.Migrations;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Helpers;
using DiscordBot.Services.Models;
using DiscordBot.Services.Services;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace DiscordBot.ServicesTests.Services;

// Shares a collection with IdentityTests/LiteDbManagerLeaseTests/HostingRepositoryTests: all
// construct LiteDbManager instances, whose ctor mutates the process-wide static BsonMapper.Global.
[Collection("LiteDbManager")]
public class HostingServiceTests : IDisposable {
	private static readonly DateTime FixedNow = new(2026, 9, 12, 0, 0, 0, DateTimeKind.Utc);

	private readonly LiteDbManager _dbManager;
	private readonly IOptions<LiteDbOptions> _options;
	private readonly IRepositoryStrategy _repositoryStrategy;

	public HostingServiceTests() {
		var managerMock = Substitute.For<MigrationManager>(NullLoggerFactory.Instance);
		var logger = new NullLogger<LiteDbManager>();
		_options = Options.Create(new LiteDbOptions {
			FileSuffix = "db",
			PathPrefix = $"tests-{Guid.NewGuid()}-"
		});

		_dbManager = new LiteDbManager(logger, _options, managerMock);
		_repositoryStrategy = new RepositoryStrategy(new IRepositoryFactory[] {
			new GuildHostingStateRepositoryFactory(NullLoggerFactory.Instance, _dbManager),
			new HostingSettingsRepositoryFactory(NullLoggerFactory.Instance, _dbManager)
		});
	}

	public void Dispose() {
		_dbManager.DisposeAll();

		var files = Directory.GetFiles(".", _options.Value.PathPrefix + "*.db");
		foreach (var file in files) {
			File.Delete(file);
		}
	}

	private sealed class FakeClock : IClock {
		public FakeClock(DateTime utcNow) => UtcNow = utcNow;
		public DateTime UtcNow { get; set; }
	}

	private static Func<double> SequenceRandom(params double[] values) {
		var queue = new Queue<double>(values);
		return () => queue.Count > 0 ? queue.Dequeue() : values[^1];
	}

	private static Func<double> ThrowingRandom() => () => throw new InvalidOperationException("random source should not have been called");

	private HostingService CreateService(MessageConfiguration? messages = null, BotTeamConfiguration? team = null, IClock? clock = null,
		Func<double>? randomSource = null) =>
		new(NullLogger<HostingService>.Instance,
			_repositoryStrategy,
			messages ?? new MessageConfiguration(),
			Options.Create(team ?? new BotTeamConfiguration()),
			clock ?? new FakeClock(FixedNow),
			randomSource);

	private void SeedPayment(DiscordGuildId guildId, DateOnly paidOn, int termMonths, bool footerEnabled = true, string? note = null,
		DiscordUserId? recordedBy = null) {
		using var repo = _repositoryStrategy.GetOrCreateRepository<IGuildHostingStateRepository>();
		repo.Insert(new GuildHostingState {
			GuildId = guildId,
			FooterEnabled = footerEnabled,
			Payments = new List<HostingPayment> {
				new() {
					PaidOn = HostingDates.ToStorage(paidOn),
					TermMonths = termMonths,
					Note = note,
					RecordedBy = recordedBy ?? new DiscordUserId(1)
				}
			}
		});
	}

	private static MessageConfiguration TestTierMessages() => new() {
		Hosting = new HostingMessages {
			Overdue = new List<HostingTier> {
				new() { MinDays = 0, Texts = new List<string> { "T0 {server} {days} {date}" } },
				new() { MinDays = 7, Texts = new List<string> { "T7 {server} {days} {date}" } },
				new() { MinDays = 30, Texts = new List<string> { "T30 {server} {days} {date}" } },
				new() { MinDays = 90, Texts = new List<string> { "T90 {server} {days} {date}" } }
			}
		}
	};

	// --- footer ---

	[Fact]
	public void NoPayment_NoNeverPaidConfig_ReturnsNull() {
		var service = CreateService();

		var status = service.GetStatus(new DiscordGuildId(1));

		status.FooterText.Should().BeNull();
		status.FooterTemplate.Should().BeNull();
	}

	[Fact]
	public void NoPayment_WithNeverPaidConfig_ReturnsText() {
		var messages = new MessageConfiguration { Hosting = new HostingMessages { NeverPaid = new List<string> { "never paid {server}" } } };
		var service = CreateService(messages: messages);

		var status = service.GetStatus(new DiscordGuildId(2), "Clan X");

		status.FooterText.Should().Be("never paid Clan X");
		status.FooterTemplate.Should().Be("never paid {server}");
		status.FooterTierMinDays.Should().BeNull();
	}

	[Fact]
	public void Paid_NotYetDue_ReturnsNull() {
		var guildId = new DiscordGuildId(3);
		SeedPayment(guildId, new DateOnly(2026, 9, 1), 1); // due 2026-10-01
		var service = CreateService();

		var status = service.GetStatus(guildId);

		status.FooterText.Should().BeNull();
		status.DaysOverdue.Should().BeLessThan(0);
	}

	[Fact]
	public void DueToday_UsesTier0() {
		var guildId = new DiscordGuildId(4);
		SeedPayment(guildId, new DateOnly(2026, 9, 12), 0); // due today
		var service = CreateService(messages: TestTierMessages());

		var status = service.GetStatus(guildId);

		status.DaysOverdue.Should().Be(0);
		status.FooterText.Should().StartWith("T0 ");
		status.FooterTemplate.Should().Be("T0 {server} {days} {date}");
		status.FooterTierMinDays.Should().Be(0);
	}

	[Fact]
	public void Overdue7_UsesTier7() {
		var guildId = new DiscordGuildId(5);
		SeedPayment(guildId, new DateOnly(2026, 9, 5), 0); // 7 days overdue
		var service = CreateService(messages: TestTierMessages());

		var status = service.GetStatus(guildId);

		status.DaysOverdue.Should().Be(7);
		status.FooterText.Should().StartWith("T7 ");
	}

	[Fact]
	public void Overdue29_StillTier7() {
		var guildId = new DiscordGuildId(6);
		SeedPayment(guildId, new DateOnly(2026, 8, 14), 0); // 29 days overdue
		var service = CreateService(messages: TestTierMessages());

		var status = service.GetStatus(guildId);

		status.DaysOverdue.Should().Be(29);
		status.FooterText.Should().StartWith("T7 ");
	}

	[Fact]
	public void Overdue30_UsesTier30() {
		var guildId = new DiscordGuildId(7);
		SeedPayment(guildId, new DateOnly(2026, 8, 13), 0); // 30 days overdue
		var service = CreateService(messages: TestTierMessages());

		var status = service.GetStatus(guildId);

		status.DaysOverdue.Should().Be(30);
		status.FooterText.Should().StartWith("T30 ");
		status.FooterTierMinDays.Should().Be(30);
	}

	[Fact]
	public void Overdue90_UsesTier90() {
		var guildId = new DiscordGuildId(80);
		SeedPayment(guildId, new DateOnly(2026, 6, 14), 0); // exactly 90 days overdue
		var service = CreateService(messages: TestTierMessages());

		var status = service.GetStatus(guildId);

		status.DaysOverdue.Should().Be(90);
		status.FooterText.Should().StartWith("T90 ");
	}

	[Fact]
	public void Overdue120_UsesTier90() {
		var guildId = new DiscordGuildId(8);
		SeedPayment(guildId, new DateOnly(2026, 5, 15), 0); // 120 days overdue
		var service = CreateService(messages: TestTierMessages());

		var status = service.GetStatus(guildId);

		status.DaysOverdue.Should().Be(120);
		status.FooterText.Should().StartWith("T90 ");
	}

	[Fact]
	public void FooterDisabled_ReturnsNull() {
		var guildId = new DiscordGuildId(9);
		SeedPayment(guildId, new DateOnly(2026, 5, 15), 0, footerEnabled: false); // very overdue, but disabled
		var service = CreateService(messages: TestTierMessages());

		var status = service.GetStatus(guildId);

		status.FooterText.Should().BeNull();
		status.FooterEnabled.Should().BeFalse();
		status.FooterTemplate.Should().BeNull();
		status.FooterTierMinDays.Should().BeNull();
	}

	[Fact]
	public void Placeholders_AreSubstituted() {
		var guildId = new DiscordGuildId(10);
		SeedPayment(guildId, new DateOnly(2026, 9, 12), 0); // due today
		var service = CreateService(messages: TestTierMessages());

		var status = service.GetStatus(guildId, "Clan Y");

		status.FooterText.Should().Be("T0 Clan Y 0 12 Sep 2026");
	}

	[Fact]
	public void RandomText_CoversAllTextsOfTier() {
		var guildId = new DiscordGuildId(11);
		SeedPayment(guildId, new DateOnly(2026, 9, 12), 0); // due today, tier 0
		var messages = new MessageConfiguration {
			Hosting = new HostingMessages {
				Overdue = new List<HostingTier> {
					new() { MinDays = 0, Texts = new List<string> { "alpha", "beta", "gamma" } }
				}
			}
		};
		var service = CreateService(messages: messages, randomSource: SequenceRandom(0.0, 0.5, 0.99));

		var seen = new HashSet<string> {
			service.GetStatus(guildId).FooterText!,
			service.GetStatus(guildId).FooterText!,
			service.GetStatus(guildId).FooterText!
		};

		seen.Should().BeEquivalentTo(new[] { "alpha", "beta", "gamma" });
	}

	// --- dates ---

	[Fact]
	public void DueOn_Jan31Plus1Month_IsFeb28() {
		var guildId = new DiscordGuildId(20);
		SeedPayment(guildId, new DateOnly(2026, 1, 31), 1);
		var service = CreateService(clock: new FakeClock(new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)));

		var status = service.GetStatus(guildId);

		status.DueOn.Should().Be(new DateOnly(2026, 2, 28));
	}

	[Fact]
	public void DaysOverdue_Negative_WhenNotDue() {
		var guildId = new DiscordGuildId(21);
		SeedPayment(guildId, new DateOnly(2026, 9, 12), 1); // due 2026-10-12
		var service = CreateService();

		var status = service.GetStatus(guildId);

		status.DaysOverdue.Should().Be(-30);
	}

	// --- degrade ---

	[Fact]
	public void ShouldDegrade_False_BelowThreshold() {
		var guildId = new DiscordGuildId(30);
		SeedPayment(guildId, new DateOnly(2026, 6, 15), 0); // 89 days overdue
		var service = CreateService(randomSource: ThrowingRandom());

		service.ShouldDegrade(guildId, new DiscordUserId(999)).Should().BeFalse();

		var state = service.GetAllStates().First(s => s.GuildId == guildId);
		state.DegradeDeniedCount.Should().Be(0);
		state.DegradeAllowedCount.Should().Be(0);
	}

	[Fact]
	public void ShouldDegrade_True_WhenDraw0_AtThreshold() {
		var guildId = new DiscordGuildId(31);
		SeedPayment(guildId, new DateOnly(2026, 5, 15), 0); // 120 days overdue, well above the 90-day threshold
		var service = CreateService(randomSource: SequenceRandom(0.0));

		service.ShouldDegrade(guildId, new DiscordUserId(999)).Should().BeTrue();

		var state = service.GetAllStates().First(s => s.GuildId == guildId);
		state.DegradeDeniedCount.Should().Be(1);
		state.DegradeAllowedCount.Should().Be(0);
	}

	[Fact]
	public void ShouldDegrade_False_WhenDraw1() {
		var guildId = new DiscordGuildId(32);
		SeedPayment(guildId, new DateOnly(2026, 5, 15), 0); // 120 days overdue
		var service = CreateService(randomSource: SequenceRandom(1.0));

		service.ShouldDegrade(guildId, new DiscordUserId(999)).Should().BeFalse();

		var state = service.GetAllStates().First(s => s.GuildId == guildId);
		state.DegradeDeniedCount.Should().Be(0);
		state.DegradeAllowedCount.Should().Be(1);
	}

	[Fact]
	public void ShouldDegrade_False_InOwnerGuild() {
		var guildId = new DiscordGuildId(33);
		SeedPayment(guildId, new DateOnly(2026, 5, 15), 0); // 120 days overdue
		var team = new BotTeamConfiguration { GuildId = guildId, OwnerId = new DiscordUserId(777) };
		var service = CreateService(team: team, randomSource: ThrowingRandom());

		service.ShouldDegrade(guildId, new DiscordUserId(999)).Should().BeFalse();

		var state = service.GetAllStates().First(s => s.GuildId == guildId);
		state.DegradeDeniedCount.Should().Be(0);
		state.DegradeAllowedCount.Should().Be(0);
	}

	[Fact]
	public void ShouldDegrade_False_ForOwner() {
		var guildId = new DiscordGuildId(34);
		SeedPayment(guildId, new DateOnly(2026, 5, 15), 0); // 120 days overdue
		var ownerId = new DiscordUserId(777);
		var team = new BotTeamConfiguration { GuildId = new DiscordGuildId(999999), OwnerId = ownerId };
		var service = CreateService(team: team, randomSource: ThrowingRandom());

		service.ShouldDegrade(guildId, ownerId).Should().BeFalse();

		var state = service.GetAllStates().First(s => s.GuildId == guildId);
		state.DegradeDeniedCount.Should().Be(0);
		state.DegradeAllowedCount.Should().Be(0);
	}

	[Fact]
	public void ShouldDegrade_False_WhenFooterDisabled() {
		var guildId = new DiscordGuildId(35);
		SeedPayment(guildId, new DateOnly(2026, 5, 15), 0, footerEnabled: false); // 120 days overdue
		var service = CreateService(randomSource: ThrowingRandom());

		service.ShouldDegrade(guildId, new DiscordUserId(999)).Should().BeFalse();

		var state = service.GetAllStates().First(s => s.GuildId == guildId);
		state.DegradeDeniedCount.Should().Be(0);
		state.DegradeAllowedCount.Should().Be(0);
	}

	[Fact]
	public void ShouldDegrade_False_WhenDisabledInConfig() {
		var guildId = new DiscordGuildId(36);
		SeedPayment(guildId, new DateOnly(2026, 5, 15), 0); // 120 days overdue
		var messages = new MessageConfiguration { Hosting = new HostingMessages { Degraded = new DegradedMode { Enabled = false } } };
		var service = CreateService(messages: messages, randomSource: ThrowingRandom());

		service.ShouldDegrade(guildId, new DiscordUserId(999)).Should().BeFalse();

		var state = service.GetAllStates().First(s => s.GuildId == guildId);
		state.DegradeDeniedCount.Should().Be(0);
		state.DegradeAllowedCount.Should().Be(0);
	}

	[Fact]
	public void ShouldDegrade_True_AtExactlyMinDaysOverdue() {
		var guildId = new DiscordGuildId(37);
		SeedPayment(guildId, new DateOnly(2026, 6, 14), 0); // exactly 90 days overdue, the default MinDaysOverdue
		var service = CreateService(randomSource: SequenceRandom(0.0));

		service.ShouldDegrade(guildId, new DiscordUserId(999)).Should().BeTrue();
	}

	[Fact]
	public void ShouldDegrade_AccumulatesCountsAcrossCalls() {
		var guildId = new DiscordGuildId(38);
		SeedPayment(guildId, new DateOnly(2026, 5, 15), 0); // 120 days overdue
		var service = CreateService(randomSource: SequenceRandom(0.0, 1.0, 0.0));

		service.ShouldDegrade(guildId, new DiscordUserId(999));
		service.ShouldDegrade(guildId, new DiscordUserId(999));
		service.ShouldDegrade(guildId, new DiscordUserId(999));

		var state = service.GetAllStates().First(s => s.GuildId == guildId);
		state.DegradeDeniedCount.Should().Be(2);
		state.DegradeAllowedCount.Should().Be(1);
	}

	// --- degraded message ---

	[Fact]
	public void GetDegradedMessage_UsesDefaultsWhenTextsNull() {
		var guildId = new DiscordGuildId(70);
		SeedPayment(guildId, new DateOnly(2026, 5, 15), 0); // 120 days overdue
		var messages = new MessageConfiguration { Hosting = new HostingMessages { Degraded = new DegradedMode { Texts = null } } };
		var service = CreateService(messages: messages, randomSource: SequenceRandom(0.0));

		var message = service.GetDegradedMessage(guildId, "Clan Z");

		var expected = HostingDefaults.Degraded[0].Replace("{server}", "Clan Z").Replace("{days}", "120");
		message.Should().Be(expected);
	}

	[Fact]
	public void GetDegradedMessage_SubstitutesPlaceholders() {
		var guildId = new DiscordGuildId(71);
		SeedPayment(guildId, new DateOnly(2026, 5, 15), 0); // 120 days overdue
		var messages = new MessageConfiguration {
			Hosting = new HostingMessages { Degraded = new DegradedMode { Texts = new List<string> { "{server} owes {days} days" } } }
		};
		var service = CreateService(messages: messages, randomSource: SequenceRandom(0.0));

		var message = service.GetDegradedMessage(guildId, "Clan Z");

		message.Should().Be("Clan Z owes 120 days");
	}

	[Fact]
	public void GetDegradedMessage_CyclesAllTexts() {
		var guildId = new DiscordGuildId(72);
		SeedPayment(guildId, new DateOnly(2026, 5, 15), 0); // 120 days overdue
		var messages = new MessageConfiguration {
			Hosting = new HostingMessages { Degraded = new DegradedMode { Texts = new List<string> { "alpha {days}", "beta {days}" } } }
		};
		var service = CreateService(messages: messages, randomSource: SequenceRandom(0.0, 0.99));

		var seen = new HashSet<string> {
			service.GetDegradedMessage(guildId, "Clan Z"),
			service.GetDegradedMessage(guildId, "Clan Z")
		};

		seen.Should().BeEquivalentTo(new[] { "alpha 120", "beta 120" });
	}

	[Fact]
	public void GetDegradedMessage_IncrementsTemplateUsage() {
		var guildId = new DiscordGuildId(73);
		SeedPayment(guildId, new DateOnly(2026, 5, 15), 0); // 120 days overdue
		var messages = new MessageConfiguration {
			Hosting = new HostingMessages { Degraded = new DegradedMode { Texts = new List<string> { "{server} owes {days} days" } } }
		};
		var service = CreateService(messages: messages, randomSource: SequenceRandom(0.0));

		service.GetDegradedMessage(guildId, "Clan Z");

		using var repo = _repositoryStrategy.GetOrCreateRepository<IHostingSettingsRepository>();
		var settings = repo.GetSingle().Value;
		settings!.TemplateUsageCounts["Degraded:-:{server} owes {days} days"].Should().Be(1);
	}

	// --- overview ---

	[Fact]
	public void GetOverview_SortsMostOverdueFirst_UnknownGuildsLast() {
		var mostOverdue = new DiscordGuildId(40);
		var slightlyOverdue = new DiscordGuildId(41);
		var notYetDue = new DiscordGuildId(42);
		var unknown = new DiscordGuildId(43);

		SeedPayment(mostOverdue, new DateOnly(2026, 5, 15), 0); // 120 days overdue
		SeedPayment(slightlyOverdue, new DateOnly(2026, 9, 2), 0); // 10 days overdue
		SeedPayment(notYetDue, new DateOnly(2026, 10, 1), 0); // not yet due (negative)

		var service = CreateService();
		var guilds = new List<Guild> {
			new() { GuildId = notYetDue, Name = "NotYetDue" },
			new() { GuildId = unknown, Name = "Unknown" },
			new() { GuildId = mostOverdue, Name = "MostOverdue" },
			new() { GuildId = slightlyOverdue, Name = "SlightlyOverdue" }
		};

		var result = service.GetOverview(guilds);

		result.IsSuccess.Should().BeTrue();
		result.Value.Select(s => s.GuildId).Should().ContainInOrder(mostOverdue, slightlyOverdue, notYetDue, unknown);
	}

	// --- write paths ---

	[Fact]
	public void RecordPayment_AppendsAndUpdatesCache() {
		var guildId = new DiscordGuildId(50);
		var service = CreateService();

		var result = service.RecordPayment(guildId, new DateOnly(2026, 9, 1), 12, "note", new DiscordUserId(5));

		result.IsSuccess.Should().BeTrue();

		var status = service.GetStatus(guildId);
		status.LastPayment.Should().NotBeNull();
		status.LastPayment!.Note.Should().Be("note");
		status.DueOn.Should().Be(new DateOnly(2027, 9, 1));

		using var repo = _repositoryStrategy.GetOrCreateRepository<IGuildHostingStateRepository>();
		var stored = repo.GetByGuildId(guildId).Value;
		stored.Should().NotBeNull();
		stored!.Payments.Should().HaveCount(1);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(61)]
	public void RecordPayment_RejectsTermOutsideRange(int termMonths) {
		var service = CreateService();

		var result = service.RecordPayment(new DiscordGuildId(51), new DateOnly(2026, 9, 1), termMonths, null, new DiscordUserId(1));

		result.IsFailed.Should().BeTrue();
	}

	[Fact]
	public void SetFooterEnabled_CreatesStateForUnknownGuild() {
		var guildId = new DiscordGuildId(52);
		var service = CreateService();

		var result = service.SetFooterEnabled(guildId, false);

		result.IsSuccess.Should().BeTrue();

		using var repo = _repositoryStrategy.GetOrCreateRepository<IGuildHostingStateRepository>();
		var stored = repo.GetByGuildId(guildId).Value;
		stored.Should().NotBeNull();
		stored!.FooterEnabled.Should().BeFalse();
	}

	[Fact]
	public void MarkUpcomingReminderSent_Persists() {
		var guildId = new DiscordGuildId(53);
		var dueOn = new DateOnly(2026, 10, 1);
		var service = CreateService();

		var result = service.MarkUpcomingReminderSent(guildId, dueOn);

		result.IsSuccess.Should().BeTrue();

		using var repo = _repositoryStrategy.GetOrCreateRepository<IGuildHostingStateRepository>();
		var stored = repo.GetByGuildId(guildId).Value;
		stored.Should().NotBeNull();
		stored!.UpcomingReminderSentForDueOn.Should().NotBeNull();
		HostingDates.ToDateOnly(stored.UpcomingReminderSentForDueOn!.Value).Should().Be(dueOn);
	}

	[Fact]
	public void MarkDueReminderSent_Persists() {
		var guildId = new DiscordGuildId(54);
		var dueOn = new DateOnly(2026, 10, 1);
		var service = CreateService();

		var result = service.MarkDueReminderSent(guildId, dueOn);

		result.IsSuccess.Should().BeTrue();

		using var repo = _repositoryStrategy.GetOrCreateRepository<IGuildHostingStateRepository>();
		var stored = repo.GetByGuildId(guildId).Value;
		stored.Should().NotBeNull();
		stored!.DueReminderSentForDueOn.Should().NotBeNull();
		HostingDates.ToDateOnly(stored.DueReminderSentForDueOn!.Value).Should().Be(dueOn);
	}

	// --- footer history ---

	[Fact]
	public void RecordFooterShown_NoOp_WhenTemplateIsNull() {
		var guildId = new DiscordGuildId(90);
		var service = CreateService();
		var status = new HostingStatus(guildId, true, null, null, null, null, null, null);

		service.RecordFooterShown(guildId, status, "status");

		service.GetAllStates().Should().NotContain(s => s.GuildId == guildId);
	}

	[Fact]
	public void RecordFooterShown_AppendsHistoryAndIncrementsUsage() {
		var guildId = new DiscordGuildId(91);
		SeedPayment(guildId, new DateOnly(2026, 9, 12), 0); // due today, tier 0
		var service = CreateService(messages: TestTierMessages());
		var status = service.GetStatus(guildId, "Clan Q");

		service.RecordFooterShown(guildId, status, "drop");

		var state = service.GetAllStates().First(s => s.GuildId == guildId);
		state.FooterHistory.Should().HaveCount(1);
		state.FooterHistory[0].DisplayText.Should().Be(status.FooterText);
		state.FooterHistory[0].Command.Should().Be("drop");

		using var repo = _repositoryStrategy.GetOrCreateRepository<IHostingSettingsRepository>();
		var settings = repo.GetSingle().Value;
		var key = $"Overdue:0:{status.FooterTemplate}";
		settings!.TemplateUsageCounts[key].Should().Be(1);
	}

	[Fact]
	public void RecordFooterShown_CapsHistoryAtFifty() {
		var guildId = new DiscordGuildId(92);
		SeedPayment(guildId, new DateOnly(2026, 9, 12), 0); // due today, tier 0
		var service = CreateService(messages: TestTierMessages());
		var status = service.GetStatus(guildId, "Clan Q");

		for (var i = 0; i < 55; i++) {
			service.RecordFooterShown(guildId, status, $"drop{i}");
		}

		var state = service.GetAllStates().First(s => s.GuildId == guildId);
		state.FooterHistory.Should().HaveCount(50);
		// 55 calls - 50 cap = 5 dropped: the oldest survivor is the 6th call (index 5), and the
		// newest is the last call made (index 54), proving this actually kept the newest 50
		// rather than some other 50.
		state.FooterHistory[0].Command.Should().Be("drop5");
		state.FooterHistory[^1].Command.Should().Be("drop54");
	}

	[Fact]
	public void RecordFooterShown_PersistenceFailure_DoesNotThrow() {
		var guildId = new DiscordGuildId(93);

		var failingStateRepo = Substitute.For<IGuildHostingStateRepository>();
		failingStateRepo.GetAll().Returns(Result.Ok<IEnumerable<GuildHostingState>>(new List<GuildHostingState>()));
		failingStateRepo.UpdateOrInsert(Arg.Any<GuildHostingState>()).Returns(Result.Fail("simulated persistence failure"));

		var failingSettingsRepo = Substitute.For<IHostingSettingsRepository>();
		failingSettingsRepo.GetSingle().Returns(Result.Ok(new HostingSettings()));
		failingSettingsRepo.UpdateOrInsert(Arg.Any<HostingSettings>()).Returns(Result.Fail("simulated persistence failure"));

		var strategy = Substitute.For<IRepositoryStrategy>();
		strategy.GetOrCreateRepository<IGuildHostingStateRepository>().Returns(failingStateRepo);
		strategy.GetOrCreateRepository<IHostingSettingsRepository>().Returns(failingSettingsRepo);

		var service = new HostingService(NullLogger<HostingService>.Instance, strategy, TestTierMessages(),
			Options.Create(new BotTeamConfiguration()), new FakeClock(FixedNow));

		var status = new HostingStatus(guildId, true, null, null, null, "some footer text", "some template", null);

		var act = () => service.RecordFooterShown(guildId, status, "drop");

		act.Should().NotThrow();
	}

	// --- resilience ---

	[Fact]
	public void GetStatus_WhenFirstLoadFails_RetriesOnNextCall() {
		var guildId = new DiscordGuildId(60);

		var failingRepo = Substitute.For<IGuildHostingStateRepository>();
		failingRepo.GetAll().Returns(Result.Fail<IEnumerable<GuildHostingState>>("simulated transient DB error"));

		var workingRepo = Substitute.For<IGuildHostingStateRepository>();
		workingRepo.GetAll().Returns(Result.Ok<IEnumerable<GuildHostingState>>(new List<GuildHostingState> {
			new() {
				GuildId = guildId,
				FooterEnabled = true,
				Payments = new List<HostingPayment> {
					new() { PaidOn = HostingDates.ToStorage(new DateOnly(2026, 5, 15)), TermMonths = 0, RecordedBy = new DiscordUserId(1) }
				}
			}
		}));

		var strategy = Substitute.For<IRepositoryStrategy>();
		strategy.GetOrCreateRepository<IGuildHostingStateRepository>().Returns(failingRepo, workingRepo);

		var service = new HostingService(NullLogger<HostingService>.Instance, strategy, TestTierMessages(),
			Options.Create(new BotTeamConfiguration()), new FakeClock(FixedNow));

		// First call: the repository fails, so the cache must NOT be marked as loaded.
		var first = service.GetStatus(guildId);
		first.LastPayment.Should().BeNull();
		first.FooterText.Should().BeNull();

		// Second call: a working repository is returned this time, so the cache must retry and
		// populate rather than staying stuck on the earlier failure.
		var second = service.GetStatus(guildId);
		second.LastPayment.Should().NotBeNull();
		second.DaysOverdue.Should().Be(120);
		second.FooterText.Should().StartWith("T90 ");
	}

	// --- template catalog ---

	[Fact]
	public void GetTemplateCatalog_ReflectsConfigAndCounts() {
		var messages = new MessageConfiguration {
			Hosting = new HostingMessages {
				NeverPaid = new List<string> { "np1" },
				Overdue = new List<HostingTier> { new() { MinDays = 0, Texts = new List<string> { "t0" } } },
				Degraded = new DegradedMode { Texts = new List<string> { "d1" } }
			}
		};
		var service = CreateService(messages: messages, randomSource: SequenceRandom(0.0));

		service.GetDegradedMessage(new DiscordGuildId(100), "X"); // increments "d1" once

		var catalog = service.GetTemplateCatalog();

		catalog.Should().ContainSingle(e => e.Kind == "NeverPaid" && e.Text == "np1" && e.UsageCount == 0);
		catalog.Should().ContainSingle(e => e.Kind == "Overdue" && e.TierMinDays == 0 && e.Text == "t0" && e.UsageCount == 0);
		catalog.Should().ContainSingle(e => e.Kind == "Degraded" && e.Text == "d1" && e.UsageCount == 1);
	}

	[Fact]
	public void GetTemplateCatalog_UsesDefaultsWhenOverdueAndDegradedEmpty_NeverPaidStaysEmpty() {
		var service = CreateService();

		var catalog = service.GetTemplateCatalog();

		catalog.Count(e => e.Kind == "Overdue").Should().Be(HostingDefaults.Overdue.Sum(t => t.Texts.Count));
		catalog.Count(e => e.Kind == "Degraded").Should().Be(HostingDefaults.Degraded.Count);
		catalog.Count(e => e.Kind == "NeverPaid").Should().Be(0);
	}
}
