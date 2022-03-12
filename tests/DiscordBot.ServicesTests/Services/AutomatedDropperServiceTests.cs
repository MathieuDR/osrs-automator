using System.Collections.ObjectModel;
using System.Text;
using AutoBogus;
using Bogus;
using DiscordBot.Common.Dtos.Runescape;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Data.Drops;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Services;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Quartz;
using Xunit;

namespace DiscordBot.ServicesTests.Services;

public class AutomatedDropperServiceTests {
    private readonly Faker<RunescapeDrop> _faker;
    private readonly Faker<RunescapeItem> _itemFaker;

    public AutomatedDropperServiceTests() {
        _itemFaker = new AutoFaker<RunescapeItem>()
            .RuleFor(x => x.Value, faker => faker.Random.Int(0, 250000))
            .RuleFor(x => x.HaValue, faker => faker.Random.Int(0, 250000))
            .RuleSet("expensive", rules => {
                rules.RuleFor(x => x.Value, faker => faker.Random.Int(1000000));
                rules.RuleFor(x => x.HaValue, faker => faker.Random.Int(1000000));
            });

        _faker = new AutoFaker<RunescapeDrop>()
            .RuleFor(x => x.Amount, faker => faker.Random.Int(1, 10))
            .Ignore(x => x.Image)
            .RuleFor(x => x.Item, _itemFaker.Generate())
            .RuleSet("expensive", rules => {
                rules.RuleFor(x => x.Amount, faker => faker.Random.Int(1, 100));
                rules.RuleFor(x => x.Item, _itemFaker.Generate("expensive"));
            });
    }

    [Fact]
    public void GeneratesCorrectDrops() {
        //Act
        var drop = _faker.Generate();

        //Assert
        drop.Amount.Should().BeGreaterOrEqualTo(1).And.BeLessOrEqualTo(10);
        drop.Item.Should().NotBeNull();
        drop.Item.Value.Should().BeGreaterOrEqualTo(0).And.BeLessOrEqualTo(250000);
        drop.Item.HaValue.Should().BeGreaterOrEqualTo(0).And.BeLessOrEqualTo(250000);
        drop.Item.Name.Should().NotBeNullOrEmpty();
        drop.Recipient.Should().NotBeNull();
        drop.Image.Should().BeNullOrEmpty();
        drop.Recipient.Username.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task WhenReceivingImageFirstItCreatesEmptyDrop() {
        //Arrange
        var repo = Substitute.For<IRuneScapeDropDataRepository>();
        var repositoryStrategy = Substitute.For<IRepositoryStrategy>();
        repositoryStrategy.GetOrCreateRepository<IRuneScapeDropDataRepository>().Returns(repo);
        var scheduler = Substitute.For<IScheduler>();
        var schedulerFactory = Substitute.For<ISchedulerFactory>();
        schedulerFactory.GetAllSchedulers().Returns(new Collection<IScheduler> {
            scheduler
        });
        // random ulong;
        var endpoint = EndpointId.New();
        var userId = DiscordUserId.Empty;
        var imageString = Convert.ToBase64String(Encoding.UTF8.GetBytes("ImageString"));

        RunescapeDropData lastUpdated = null;
        repo.GetActive(Arg.Is(userId)).Returns(Result.Ok<RunescapeDropData>(null));
        repo.UpdateOrInsert(Arg.Do<RunescapeDropData>(a => {
            repo.GetActive(Arg.Is(userId)).Returns(Result.Ok(a));
            lastUpdated = a;
        }));

        scheduler.ScheduleJob(Arg.Do<IJobDetail>(detail => {
            var key = detail.Key;
            scheduler.CheckExists(Arg.Is<JobKey>(jobKey => jobKey.Name == key.Name)).Returns(true);
            //scheduler.
        }), Arg.Do<ITrigger>(trigger => {
            var key = trigger.Key;
            scheduler.GetTriggersOfJob(Arg.Any<JobKey>()).Returns(new List<ITrigger> { trigger });
        }));

        scheduler.CheckExists(Arg.Any<JobKey>()).Returns(false);

        var sut = new AutomatedDropperService(Substitute.For<ILogger<AutomatedDropperService>>(),
            repositoryStrategy, schedulerFactory);
        var drop = _faker.Generate();

        //Act
        var firstRun = await sut.HandleDropRequest(endpoint, null, imageString);

        //Assert
        firstRun.IsSuccess.Should().BeTrue(string.Join(", ", firstRun.Errors));
        lastUpdated.Should().NotBeNull();
        lastUpdated.Endpoint.Should().Be(endpoint);
        lastUpdated.Drops.Should().HaveCount(1, "We sent one drop info");
        lastUpdated.Drops.First().Image.Should().Be(imageString);
        lastUpdated.Drops.First().Amount.Should().Be(0);
        lastUpdated.Drops.First().Item.Should().Be(null);
        lastUpdated.Drops.First().Recipient.Should().BeEquivalentTo(default(RunescapeDrop.Player));
    }

    [Fact]
    public void CanInitService() {
        //Arrange
        var repo = Substitute.For<IRuneScapeDropDataRepository>();
        var repositoryStrategy = Substitute.For<IRepositoryStrategy>();
        repositoryStrategy.GetOrCreateRepository<IRuneScapeDropDataRepository>().Returns(repo);
        var scheduler = Substitute.For<IScheduler>();
        var schedulerFactory = Substitute.For<ISchedulerFactory>();
        schedulerFactory.GetAllSchedulers().Returns(new Collection<IScheduler> {
            scheduler
        });

        //Act
        var sut = new AutomatedDropperService(Substitute.For<ILogger<AutomatedDropperService>>(),
            repositoryStrategy, schedulerFactory);

        //Assert
        sut.Should().NotBeNull();
    }

    [Fact]
    public async Task WhenReceivingImageFirstAndThenDropItMerges() {
        //Arrange
        var repo = Substitute.For<IRuneScapeDropDataRepository>();
        var repositoryStrategy = Substitute.For<IRepositoryStrategy>();
        repositoryStrategy.GetOrCreateRepository<IRuneScapeDropDataRepository>().Returns(repo);
        var scheduler = Substitute.For<IScheduler>();
        var schedulerFactory = Substitute.For<ISchedulerFactory>();
        schedulerFactory.GetAllSchedulers().Returns(new Collection<IScheduler> {
            scheduler
        });
        
        var endpoint = EndpointId.New();
        var userId = DiscordUserId.Empty;
        var imageString = Convert.ToBase64String(Encoding.UTF8.GetBytes("ImageString"));

        RunescapeDropData lastUpdated = null;
        repo.GetActive(Arg.Is(userId)).Returns(Result.Ok<RunescapeDropData>(null));
        repo.UpdateOrInsert(Arg.Do<RunescapeDropData>(a => {
            repo.GetActive(Arg.Is(userId)).Returns(Result.Ok(a));
            lastUpdated = a;
        }));

        scheduler.ScheduleJob(Arg.Do<IJobDetail>(detail => {
            var key = detail.Key;
            scheduler.CheckExists(Arg.Is<JobKey>(jobKey => jobKey.Name == key.Name)).Returns(true);
            //scheduler.
        }), Arg.Do<ITrigger>(trigger => {
            var key = trigger.Key;
            scheduler.GetTriggersOfJob(Arg.Any<JobKey>()).Returns(new List<ITrigger> { trigger });
        }));

        scheduler.CheckExists(Arg.Any<JobKey>()).Returns(false);

        var sut = new AutomatedDropperService(Substitute.For<ILogger<AutomatedDropperService>>(),
            repositoryStrategy, schedulerFactory);
        var drop = _faker.Generate();

        //Act
        var firstRun = await sut.HandleDropRequest(endpoint, null, imageString);
        var secondRun = await sut.HandleDropRequest(endpoint, drop, null);

        //Assert
        firstRun.IsSuccess.Should().BeTrue(string.Join(", ", firstRun.Errors));
        secondRun.IsSuccess.Should().BeTrue(string.Join(", ", secondRun.Errors));
        lastUpdated.Should().NotBeNull();
        lastUpdated.Endpoint.Should().Be(endpoint);
        lastUpdated.Drops.Should().HaveCount(1, "We sent one drop info");
        lastUpdated.Drops.First().Image.Should().Be(imageString);
        lastUpdated.Drops.First().Amount.Should().Be(drop.Amount);
        lastUpdated.Drops.First().Item.Should().Be(drop.Item);
        lastUpdated.PlayerDrops.Should().HaveCount(1, "One player drop");
    }

    [Fact]
    public async Task WhenReceivingDropAndImageItMerges() {
        var repo = Substitute.For<IRuneScapeDropDataRepository>();
        var repositoryStrategy = Substitute.For<IRepositoryStrategy>();
        repositoryStrategy.GetOrCreateRepository<IRuneScapeDropDataRepository>().Returns(repo);
        var scheduler = Substitute.For<IScheduler>();
        var schedulerFactory = Substitute.For<ISchedulerFactory>();
        schedulerFactory.GetAllSchedulers().Returns(new Collection<IScheduler> {
            scheduler
        });
        
        var endpoint = EndpointId.New();
        var userId = DiscordUserId.Empty;
        var imageString = Convert.ToBase64String(Encoding.UTF8.GetBytes("ImageString"));

        RunescapeDropData lastUpdated = null;
        repo.GetActive(Arg.Is(userId)).Returns(Result.Ok<RunescapeDropData>(null));
        repo.UpdateOrInsert(Arg.Do<RunescapeDropData>(a => {
            repo.GetActive(Arg.Is(userId)).Returns(Result.Ok(a));
            lastUpdated = a;
        }));

        scheduler.ScheduleJob(Arg.Do<IJobDetail>(detail => {
            var key = detail.Key;
            scheduler.CheckExists(Arg.Is<JobKey>(jobKey => jobKey.Name == key.Name)).Returns(true);
            //scheduler.
        }), Arg.Do<ITrigger>(trigger => {
            var key = trigger.Key;
            scheduler.GetTriggersOfJob(Arg.Any<JobKey>()).Returns(new List<ITrigger> { trigger });
        }));

        scheduler.CheckExists(Arg.Any<JobKey>()).Returns(false);

        var sut = new AutomatedDropperService(Substitute.For<ILogger<AutomatedDropperService>>(),
            repositoryStrategy, schedulerFactory);
        var drop = _faker.Generate();

        //Act
        var firstRun = await sut.HandleDropRequest(endpoint, drop, null);
        var secondRun = await sut.HandleDropRequest(endpoint, null, imageString);

        //Assert
        firstRun.IsSuccess.Should().BeTrue(string.Join(", ", firstRun.Errors));
        secondRun.IsSuccess.Should().BeTrue(string.Join(", ", secondRun.Errors));
        lastUpdated.Should().NotBeNull();
        lastUpdated.Endpoint.Should().Be(endpoint);
        lastUpdated.Drops.Should().HaveCount(1, "We sent one drop info");
        lastUpdated.Drops.First().Image.Should().Be(imageString);
        lastUpdated.Drops.First().Amount.Should().Be(drop.Amount);
        lastUpdated.Drops.First().Item.Should().Be(drop.Item);
    }

    [Fact]
    public async Task WhenReceivingDropItSetsTask() {
        var repo = Substitute.For<IRuneScapeDropDataRepository>();
        var repositoryStrategy = Substitute.For<IRepositoryStrategy>();
        repositoryStrategy.GetOrCreateRepository<IRuneScapeDropDataRepository>().Returns(repo);
        var scheduler = Substitute.For<IScheduler>();
        var schedulerFactory = Substitute.For<ISchedulerFactory>();
        schedulerFactory.GetAllSchedulers().Returns(new Collection<IScheduler> {
            scheduler
        });
        
        var endpoint = EndpointId.New();
        var userId = DiscordUserId.Empty;
        RunescapeDropData lastUpdated = null;
        repo.GetActive(Arg.Is(userId)).Returns(Result.Ok<RunescapeDropData>(null));
        scheduler.CheckExists(Arg.Any<JobKey>()).Returns(false);

        var sut = new AutomatedDropperService(Substitute.For<ILogger<AutomatedDropperService>>(),
            repositoryStrategy, schedulerFactory);
        var drop = _faker.Generate();

        //Act
        _ = await sut.HandleDropRequest(endpoint, drop, null);

        //Assert
        scheduler.Received(1).ScheduleJob(Arg.Is<IJobDetail>(x => x.Key.Name == userId.ToString()), Arg.Any<ITrigger>());
    }

    [Fact]
    public async Task WhenReceivingMultipleDropsItOnlySetsOneTask() {
        var repo = Substitute.For<IRuneScapeDropDataRepository>();
        var repositoryStrategy = Substitute.For<IRepositoryStrategy>();
        repositoryStrategy.GetOrCreateRepository<IRuneScapeDropDataRepository>().Returns(repo);
        var scheduler = Substitute.For<IScheduler>();
        var schedulerFactory = Substitute.For<ISchedulerFactory>();
        schedulerFactory.GetAllSchedulers().Returns(new Collection<IScheduler> {
            scheduler
        });
        
        var endpoint = EndpointId.New();
        var userId = DiscordUserId.Empty;
        RunescapeDropData lastUpdated = null;
        repo.GetActive(Arg.Is(userId)).Returns(Result.Ok<RunescapeDropData>(null));
        repo.UpdateOrInsert(Arg.Do<RunescapeDropData>(a => {
            repo.GetActive(Arg.Is(userId)).Returns(Result.Ok(a));
            lastUpdated = a;
        }));

        scheduler.ScheduleJob(Arg.Do<IJobDetail>(detail => {
            var key = detail.Key;
            scheduler.CheckExists(Arg.Is<JobKey>(jobKey => jobKey.Name == key.Name)).Returns(true);
            //scheduler.
        }), Arg.Do<ITrigger>(trigger => {
            var key = trigger.Key;
            scheduler.GetTriggersOfJob(Arg.Any<JobKey>()).Returns(new List<ITrigger> { trigger });
        }));

        scheduler.CheckExists(Arg.Any<JobKey>()).Returns(false);

        var sut = new AutomatedDropperService(Substitute.For<ILogger<AutomatedDropperService>>(),
            repositoryStrategy, schedulerFactory);
        var drop = _faker.Generate();

        //Act
        var firstRun = await sut.HandleDropRequest(endpoint, drop, null);
        var secondRun = await sut.HandleDropRequest(endpoint, drop, null);

        //Assert
        await scheduler.Received(1).ScheduleJob(Arg.Is<IJobDetail>(x => x.Key.Name == endpoint.ToString()), Arg.Any<ITrigger>());
        await scheduler.Received(1).RescheduleJob(Arg.Any<TriggerKey>(), Arg.Any<ITrigger>());
    }

    [Fact]
    public async Task WhenReceivingDropItSavesItToDb() {
        var repo = Substitute.For<IRuneScapeDropDataRepository>();
        var repositoryStrategy = Substitute.For<IRepositoryStrategy>();
        repositoryStrategy.GetOrCreateRepository<IRuneScapeDropDataRepository>().Returns(repo);
        var scheduler = Substitute.For<IScheduler>();
        var schedulerFactory = Substitute.For<ISchedulerFactory>();
        schedulerFactory.GetAllSchedulers().Returns(new Collection<IScheduler> {
            scheduler
        });

        var endpoint = EndpointId.New();
        var userId = DiscordUserId.Empty;
        RunescapeDropData lastUpdated = null;
        repo.GetActive(Arg.Is(userId)).Returns(Result.Ok<RunescapeDropData>(null));


        scheduler.CheckExists(Arg.Any<JobKey>()).Returns(false);

        var sut = new AutomatedDropperService(Substitute.For<ILogger<AutomatedDropperService>>(),
            repositoryStrategy, schedulerFactory);
        var drop = _faker.Generate();

        //Act
        var firstRun = await sut.HandleDropRequest(endpoint, drop, null);

        //Assert
        repo.Received(1).UpdateOrInsert(Arg.Any<RunescapeDropData>());
    }

    [Fact]
    public async Task WhenReceivingDropsItUsesActiveDropFromDb() {
        var repo = Substitute.For<IRuneScapeDropDataRepository>();
        var repositoryStrategy = Substitute.For<IRepositoryStrategy>();
        repositoryStrategy.GetOrCreateRepository<IRuneScapeDropDataRepository>().Returns(repo);
        var scheduler = Substitute.For<IScheduler>();
        var schedulerFactory = Substitute.For<ISchedulerFactory>();
        schedulerFactory.GetAllSchedulers().Returns(new Collection<IScheduler> {
            scheduler
        });
        
        var endpoint = EndpointId.New();
        var userId = DiscordUserId.Empty;
        RunescapeDropData lastUpdated = null;
        repo.GetActive(Arg.Is(userId)).Returns(Result.Ok<RunescapeDropData>(null));


        scheduler.CheckExists(Arg.Any<JobKey>()).Returns(false);

        var sut = new AutomatedDropperService(Substitute.For<ILogger<AutomatedDropperService>>(),
            repositoryStrategy, schedulerFactory);
        var drop = _faker.Generate();

        //Act
        var firstRun = await sut.HandleDropRequest(endpoint, drop, null);

        //Assert
        repo.Received(1).GetActive(Arg.Is(userId));
    }
}
