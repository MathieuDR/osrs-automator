using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data.Hosting;
using DiscordBot.Data;
using DiscordBot.Data.Configuration;
using DiscordBot.Data.Factories;
using DiscordBot.Data.Repository.Migrations;
using FluentAssertions;
using LiteDB;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace DiscordBot.ServicesTests.Data;

// Shares a collection with IdentityTests/LiteDbManagerLeaseTests: all construct LiteDbManager
// instances, whose ctor mutates the process-wide static BsonMapper.Global. Running them in the
// same xUnit collection forces them to run sequentially relative to each other.
[Collection("LiteDbManager")]
public class HostingRepositoryTests : IDisposable {
	private readonly LiteDbManager _dbManager;
	private readonly IOptions<LiteDbOptions> _options;

	public HostingRepositoryTests() {
		var managerMock = Substitute.For<MigrationManager>(NullLoggerFactory.Instance);
		var logger = new NullLogger<LiteDbManager>();
		_options = Options.Create(new LiteDbOptions {
			FileSuffix = "db",
			PathPrefix = $"tests-{Guid.NewGuid()}-"
		});

		_dbManager = new LiteDbManager(logger, _options, managerMock);
	}

	public void Dispose() {
		_dbManager.DisposeAll();

		var files = Directory.GetFiles(".", _options.Value.PathPrefix + "*.db");
		foreach (var file in files) {
			File.Delete(file);
		}
	}

	private GuildHostingStateRepositoryFactory CreateGuildHostingStateFactory() => new(NullLoggerFactory.Instance, _dbManager);
	private HostingSettingsRepositoryFactory CreateHostingSettingsFactory() => new(NullLoggerFactory.Instance, _dbManager);

	[Fact]
	public void GuildHostingState_RoundTrips() {
		//Arrange
		var guildId = new DiscordGuildId(555);
		var recordedBy = new DiscordUserId(42);
		var state = new GuildHostingState {
			GuildId = guildId,
			FooterEnabled = false,
			Payments = new List<HostingPayment> {
				new() { PaidOn = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), TermMonths = 12, RecordedBy = recordedBy },
				new() {
					PaidOn = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc), TermMonths = 6, Note = "partial payment",
					RecordedBy = recordedBy
				}
			},
			UpcomingReminderSentForDueOn = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
			DueReminderSentForDueOn = new DateTime(2026, 12, 15, 0, 0, 0, DateTimeKind.Utc)
		};

		using (var repo = CreateGuildHostingStateFactory().Create()) {
			repo.Insert(state);
		}

		//Act
		GuildHostingState? read;
		using (var repo = CreateGuildHostingStateFactory().Create()) {
			read = repo.GetByGuildId(guildId).Value;
		}

		//Assert
		read.Should().NotBeNull();
		read.Should().BeEquivalentTo(state, opts => opts
			.Excluding(x => x.Id)
			.Excluding(x => x.CreatedOn)
			.Using<DateTime>(ctx => ctx.Subject.ToUniversalTime().Should().BeCloseTo(ctx.Expectation.ToUniversalTime(), TimeSpan.FromSeconds(1)))
			.WhenTypeIs<DateTime>()
			.Using<DateTime?>(ctx => ctx.Subject!.Value.ToUniversalTime().Should().BeCloseTo(ctx.Expectation!.Value.ToUniversalTime(), TimeSpan.FromSeconds(1)))
			.WhenTypeIs<DateTime?>());
	}

	[Fact]
	public void GetByGuildId_UnknownGuild_ReturnsNull() {
		//Arrange
		using var repo = CreateGuildHostingStateFactory().Create();

		//Act
		var result = repo.GetByGuildId(new DiscordGuildId(999999));

		//Assert
		result.Value.Should().BeNull();
	}

	[Fact]
	public void GuildId_IsUnique() {
		//Arrange
		var guildId = new DiscordGuildId(777);
		using var repo = CreateGuildHostingStateFactory().Create();
		repo.Insert(new GuildHostingState { GuildId = guildId });

		//Act
		Action act = () => repo.Insert(new GuildHostingState { GuildId = guildId });

		//Assert
		act.Should().Throw<LiteException>();
	}

	[Theory]
	[InlineData(123L)]
	[InlineData(null)]
	public void HostingSettings_RoundTrips(long? reminderChannelId) {
		//Arrange
		var settings = new HostingSettings { ReminderChannelId = reminderChannelId };

		using (var repo = CreateHostingSettingsFactory().Create()) {
			repo.Insert(settings);
		}

		//Act
		HostingSettings? read;
		using (var repo = CreateHostingSettingsFactory().Create()) {
			read = repo.GetSingle().Value;
		}

		//Assert
		read.Should().NotBeNull();
		read.Should().BeEquivalentTo(settings, opts => opts.Excluding(x => x.Id).Excluding(x => x.CreatedOn));
	}
}
