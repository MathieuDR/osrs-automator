using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data.Configuration;
using DiscordBot.Data;
using DiscordBot.Data.Configuration;
using DiscordBot.Data.Factories;
using DiscordBot.Data.Repository.Migrations;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace DiscordBot.ServicesTests.Data;

// Shares a collection with IdentityTests: both construct LiteDbManager instances, whose ctor
// mutates the process-wide static BsonMapper.Global. Running them in different xUnit collections
// (the default) lets that construction race across threads and corrupts LiteDB's own state,
// surfacing as unrelated exceptions inside the LiteDB engine. See the comment on IdentityTests.
[Collection("LiteDbManager")]
public class LiteDbManagerLeaseTests : IDisposable {
	private readonly LiteDbManager _dbManager;
	private readonly IOptions<LiteDbOptions> _options;

	public LiteDbManagerLeaseTests() {
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

	private LiteDbManager CreateManager(bool closeWhenUnused) {
		var managerMock = Substitute.For<MigrationManager>(NullLoggerFactory.Instance);
		var logger = new NullLogger<LiteDbManager>();
		return new LiteDbManager(logger, Options.Create(new LiteDbOptions {
			FileSuffix = _options.Value.FileSuffix,
			PathPrefix = _options.Value.PathPrefix,
			CloseWhenUnused = closeWhenUnused
		}), managerMock);
	}

	private string PathFor(DiscordGuildId guildId) => $"{_options.Value.PathPrefix}{guildId}_{_options.Value.FileSuffix}.db";
	private string CommonPath => $"{_options.Value.PathPrefix}common_{_options.Value.FileSuffix}.db";

	private static bool FileIsUnlocked(string path) {
		try {
			using var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
			return true;
		} catch (IOException) {
			return false;
		}
	}

	[Fact]
	public void Lease_ThenDispose_ReleasesFile() {
		//Arrange
		var guildId = new DiscordGuildId(10);
		var lease = _dbManager.Lease(guildId);
		var collection = lease.Database.GetCollection<TestModel>("m");
		collection.Insert(new TestModel { Name = "x", UserId = new DiscordUserId(1) });

		//Act
		lease.Dispose();

		//Assert
		FileIsUnlocked(PathFor(guildId)).Should().BeTrue();
		_dbManager.OpenDatabaseCount.Should().Be(0);
	}

	[Fact]
	public void Lease_WhileHeld_LocksFile() {
		//Arrange
		var guildId = new DiscordGuildId(10);

		//Act
		using var lease = _dbManager.Lease(guildId);

		//Assert
		FileIsUnlocked(PathFor(guildId)).Should().BeFalse();
		_dbManager.OpenDatabaseCount.Should().Be(1);
	}

	[Fact]
	public void TwoLeases_SameGuild_ShareInstance() {
		//Arrange
		var guildId = new DiscordGuildId(10);
		var a = _dbManager.Lease(guildId);
		var b = _dbManager.Lease(guildId);

		//Assert
		b.Database.Should().BeSameAs(a.Database);

		//Act
		a.Dispose();

		//Assert
		b.Database.GetCollection<TestModel>("m").Count().Should().Be(0);
		_dbManager.OpenDatabaseCount.Should().Be(1);

		//Act
		b.Dispose();

		//Assert
		FileIsUnlocked(PathFor(guildId)).Should().BeTrue();
	}

	[Fact]
	public void Leases_DifferentGuilds_OpenSeparateDatabases() {
		//Arrange
		using var a = _dbManager.Lease(new DiscordGuildId(1));
		using var b = _dbManager.Lease(new DiscordGuildId(2));

		//Assert
		_dbManager.OpenDatabaseCount.Should().Be(2);
		b.Database.Should().NotBeSameAs(a.Database);
	}

	[Fact]
	public void Dispose_Twice_DoesNotUnderflow() {
		//Arrange
		var guildId = new DiscordGuildId(10);
		var lease = _dbManager.Lease(guildId);

		//Act
		lease.Dispose();
		lease.Dispose();

		using (var second = _dbManager.Lease(guildId)) {
			second.Database.GetCollection<TestModel>("m").Insert(new TestModel { Name = "x", UserId = new DiscordUserId(1) });
			_dbManager.OpenDatabaseCount.Should().Be(1);
		}

		//Assert
		_dbManager.OpenDatabaseCount.Should().Be(0);
	}

	[Fact]
	public void CommonLease_BehavesLikeGuildLease() {
		//Act
		var lease = _dbManager.LeaseCommon();

		//Assert
		lease.FilePath.Should().Be(CommonPath);
		FileIsUnlocked(CommonPath).Should().BeFalse();

		//Act
		lease.Dispose();

		//Assert
		FileIsUnlocked(CommonPath).Should().BeTrue();
	}

	[Fact]
	public void CloseWhenUnused_False_KeepsDatabaseOpen() {
		//Arrange
		var mgr = CreateManager(false);
		var guildId = new DiscordGuildId(10);

		//Act
		var lease = mgr.Lease(guildId);
		lease.Dispose();

		//Assert
		FileIsUnlocked(PathFor(guildId)).Should().BeFalse();
		mgr.OpenDatabaseCount.Should().Be(1);

		//Act
		mgr.DisposeAll();

		//Assert
		FileIsUnlocked(PathFor(guildId)).Should().BeTrue();
		mgr.OpenDatabaseCount.Should().Be(0);
	}

	[Fact]
	public void Parallel_Access_SameGuilds_IsSafe() {
		//Act
		Parallel.For(0, 200, i => {
			using var l = _dbManager.Lease(new DiscordGuildId((ulong)(i % 10)));
			var c = l.Database.GetCollection<TestModel>("m");
			c.Insert(new TestModel { Name = i.ToString(), UserId = new DiscordUserId(1) });
			c.Count().Should().BeGreaterThan(0);
		});

		//Assert
		_dbManager.OpenDatabaseCount.Should().Be(0);
		for (ulong g = 0; g < 10; g++) {
			FileIsUnlocked(PathFor(new DiscordGuildId(g))).Should().BeTrue();
		}
	}

	[Fact]
	public void Repository_Dispose_ReleasesFile() {
		//Arrange
		var guildId = new DiscordGuildId(10);
		var factory = new GuildConfigLiteDbRepositoryFactory(NullLoggerFactory.Instance, _dbManager);
		var repo = factory.Create(guildId);

		//Act
		repo.GetSingle();
		repo.Dispose();

		//Assert
		FileIsUnlocked(PathFor(guildId)).Should().BeTrue();
	}

	[Fact]
	public void GetAll_ReturnsMaterialisedList() {
		//Arrange
		var guildId = new DiscordGuildId(10);
		var factory = new GuildConfigLiteDbRepositoryFactory(NullLoggerFactory.Instance, _dbManager);
		var repo = factory.Create(guildId);
		repo.Insert(new GuildConfig(guildId, new DiscordUserId(1)));
		repo.Insert(new GuildConfig(guildId, new DiscordUserId(2)));

		//Act
		var all = repo.GetAll().Value;
		repo.Dispose();

		//Assert
		all.Count().Should().Be(2);
	}

	[Fact]
	public void DataPersists_AcrossLeases() {
		//Arrange
		var guildId = new DiscordGuildId(10);
		using (var a = _dbManager.Lease(guildId)) {
			a.Database.GetCollection<TestModel>("m").Insert(new TestModel { Name = "persisted", UserId = new DiscordUserId(1) });
		}

		//Act
		using var b = _dbManager.Lease(guildId);
		var read = b.Database.GetCollection<TestModel>("m").FindAll().FirstOrDefault();

		//Assert
		read.Should().NotBeNull();
		read!.Name.Should().Be("persisted");
	}
}
