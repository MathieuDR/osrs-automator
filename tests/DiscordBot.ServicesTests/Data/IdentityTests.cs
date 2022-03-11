using System.Collections;
using System.Diagnostics;
using DiscordBot.Common.Identities;
using DiscordBot.Data;
using DiscordBot.Data.Configuration;
using DiscordBot.Data.Repository.Migrations;
using FluentAssertions;
using LiteDB;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace DiscordBot.ServicesTests.Data;

public class TestModel {
	[BsonId]
	public ObjectId Id { get; set; }

	public DiscordUserId UserId { get; set; }
	public string Name { get; set; }
}

public class TestModelWithDictionary {
	[BsonId]
	public ObjectId Id { get; set; }

	public Dictionary<DiscordUserId, List<TestModel>> Dictionary { get; set; }
}

public class IdentityTests : IDisposable {
	protected LiteDbManager _dbManager;

	public IdentityTests() {
		var managerMock = Substitute.For<MigrationManager>(NullLoggerFactory.Instance);
		var logger = new NullLogger<LiteDbManager>();
		var options = Options.Create(new LiteDbOptions {
			FileSuffix = "db",
			PathPrefix = "tests-"
		});
		
		LiteDbManager.AddDictMapper<DiscordUserId, List<TestModel>>(arg => new DiscordUserId(arg));
		_dbManager = new LiteDbManager(logger, options, managerMock);
	}

	public void Dispose() {
		_dbManager.Dispose();
	}

	[Fact]
	public void GetDatabase_ShouldCreateValidDatabase() {
		//Arrange

		//Act
		var db = _dbManager.GetDatabase(new DiscordGuildId(10));

		//Assert
		db.Should().NotBeNull();
	}

	[Fact]
	public void Inserting_ShouldBeSuccessful_WhenUsingAnMappedIdentity() {
		//Arrange
		using var db = _dbManager.GetDatabase(new DiscordGuildId(10));

		//Act
		var coll = db.GetCollection<TestModel>("Models");
		var written = coll.Insert(new TestModel {
			Name = "MyTestName",
			UserId = new DiscordUserId(41232)
		});

		//Assert
		written.Should().NotBeNull();
	}


	[Fact]
	public void Reading_ShouldBeSuccessful_WhenUsingAnMappedIdentity() {
		//Arrange
		using var db = _dbManager.GetDatabase(new DiscordGuildId(10));
		var model = new TestModel {
			Name = "MyTestName",
			UserId = new DiscordUserId(513512)
		};

		//Act
		var coll = db.GetCollection<TestModel>("Models");
		var read = coll.Find(x => x.UserId == model.UserId).FirstOrDefault();

		//Assert
		read.Should().NotBeNull();
		read.Should().BeEquivalentTo(model, opts => opts.Excluding(x => x.Id));
	}

	[Fact]
	public void Inserting_ShouldBeSuccessful_WhenUsingAnMappedDictWithIdentityKey() {
		//Arrange
		using var db = _dbManager.GetDatabase(new DiscordGuildId(10));
		var model = new TestModel {
			Name = "MyTestName",
			UserId = new DiscordUserId(513512)
		};
		var model2 = new TestModel {
			Name = "MyTestName2",
			UserId = new DiscordUserId(5139512)
		};
		var model3 = new TestModel {
			Name = "MyTestName3",
			UserId = new DiscordUserId(5139512)
		};

		var dict = new Dictionary<DiscordUserId, List<TestModel>> {
			{ model.UserId, new() { model } },
			{ model2.UserId, new() { model2, model3 } }
		};
		var toWrite = new TestModelWithDictionary {
			Dictionary = dict
		};

		//Act
		var coll = db.GetCollection<TestModelWithDictionary>("Dicts");
		var written = coll.Insert(toWrite);

		//Assert
		written.Should().NotBeNull();
	}


	[Fact]
	public void Reading_ShouldBeSuccessful_WhenUsingAnMappedDictWithIdentityKey() {
		//Arrange
		using var db = _dbManager.GetDatabase(new DiscordGuildId(10));
		var model = new TestModel {
			Name = "MyTestName",
			UserId = new DiscordUserId(513512)
		};
		var model2 = new TestModel {
			Name = "MyTestName2",
			UserId = new DiscordUserId(5139512)
		};
		var model3 = new TestModel {
			Name = "MyTestName3",
			UserId = new DiscordUserId(5139512)
		};

		var dict = new Dictionary<DiscordUserId, List<TestModel>> {
			{ model.UserId, new() { model } },
			{ model2.UserId, new() { model2, model3 } }
		};
		var toWrite = new TestModelWithDictionary {
			Dictionary = dict
		};

		//Act
		var coll = db.GetCollection<TestModelWithDictionary>("Dicts");
		var read = coll.FindAll().FirstOrDefault();

		//Assert
		read.Should().NotBeNull();
		read.Should().BeEquivalentTo(toWrite, opts => opts.Excluding(x => x.Id));
	}
}
