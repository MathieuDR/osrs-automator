using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Dtos.Runescape;
using DiscordBot.Common.Identities;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Data.Drops;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Interfaces;
using DiscordBot.Services.Jobs;
using FluentResults;
using Microsoft.Extensions.Logging;
using Quartz;

namespace DiscordBot.Services.Services;

internal class AutomatedDropperService : RepositoryService, IAutomatedDropperService {
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly int _clueWaitTimeInSeconds = 60;
    private readonly int _waitTimeInSeconds = 10;

    public AutomatedDropperService(ILogger<AutomatedDropperService> logger, IRepositoryStrategy repositoryStrategy,
        ISchedulerFactory schedulerFactory) : base(logger, repositoryStrategy) {
        _schedulerFactory = schedulerFactory;
    }

    public async Task<Result<string>> RequestUrl(GuildUser user) {
        // get RunescapeDropperGuildConfiguration
        var guildConfigurationResult = GetGuildConfiguration(user.GuildId, user.Id);
        if (guildConfigurationResult.IsFailed) {
            return Result.Fail<string>("Could not get your endpoint")
                .WithErrors(guildConfigurationResult.Errors);
        }
        
        var guildConfiguration = guildConfigurationResult.Value;
        
        // check if user is disabled
        if (guildConfiguration.DisabledUsers.Contains(user.Id)) {
            return Result.Fail<string>("You are unable to use this service, you have been disabled by an admin");
        }
        
        // check if user already has an endpoint
        if(guildConfiguration.UserEndpoints.ContainsKey(user.Id)) {
            // We would like to reset
            guildConfiguration.UserEndpoints.Remove(user.Id);
        }
        
        // get a new endpoint
        var endpoint = EndpointId.New();
        guildConfiguration.UserEndpoints.Add(user.Id, endpoint);
        
        // save the endpoint
        var saveResult = SaveGuildConfiguration(guildConfiguration);
        if (saveResult.IsFailed) {
            return Result.Fail<string>("Could not save your endpoint to the database")
                .WithErrors(guildConfigurationResult.Errors);
        }
        
        return Result.Ok(CreateEndpointUri(endpoint));
    }

    private Result SaveGuildConfiguration(RunescapeDropperGuildConfiguration guildConfiguration) {
        var repo = RepositoryStrategy.GetOrCreateRepository<IRunescapeDropperGuildConfigurationRepository>(guildConfiguration.GuildId);
        return repo.UpdateOrInsert(guildConfiguration);
    }

    private string CreateEndpointUri(EndpointId endpoint) {
        // create uri
        var uri = new UriBuilder("https", "localhost", 80, $"api/v1.0/AutomatedDropper/dropper/{endpoint.Value}");
        return uri.ToString();
    }

    private Result<RunescapeDropperGuildConfiguration> GetGuildConfiguration(DiscordGuildId guildId, DiscordUserId requestingUser) {
        var repo = RepositoryStrategy.GetOrCreateRepository<IRunescapeDropperGuildConfigurationRepository>(guildId);
        var result = repo.GetSingle();

        if (result.IsFailed) {
            return result;
        }

        if (result.Value is null) {
            // TODO: Remvo the create
            return Result.Ok(new RunescapeDropperGuildConfiguration(guildId, requestingUser));
            return Result.Fail("No RunescapeDropperGuildConfiguration found");
        }
        
        return Result.Ok(result.Value);
    }

    public async Task<Result> HandleDropRequest(EndpointId endpointId, RunescapeDrop drop, string base64Image) {
        if (drop is null && string.IsNullOrEmpty(base64Image)) {
            return Result.Fail("No new information");
        }

        //Check user Id
        var allowedCheckResult = IsValidEndpoint(endpointId);
        if (allowedCheckResult.IsFailed) {
            Logger.LogInformation("Not allowed endpoint: {endpoint}", endpointId.ToString());
            return allowedCheckResult.ToResult();
        }

        var userId = allowedCheckResult.Value;

        //Save to DB
        var saveInformationResult = SaveDropData(endpointId, userId, drop, base64Image);
        if (saveInformationResult.IsFailed) {
            Logger.LogWarning("Could not save drop with endpoint {endpoint}, data: {@drop} and {verb} image", endpointId.ToString(), drop,
                string.IsNullOrEmpty(base64Image) ? "without" : "with");
            return saveInformationResult.ToResult();
        }

        //Schedule job
        var schedulingResult = await ScheduleJob(userId, saveInformationResult.Value);
        if (schedulingResult.IsFailed) {
            Logger.LogWarning("Could not schedule job with id {endpoint}", endpointId.ToString());
            return schedulingResult;
        }

        return Result.Ok();
    }

    private Result<RunescapeDrop> SaveDropData(EndpointId endpoint, DiscordUserId userId, RunescapeDrop drop, string base64Image) {
        var repo = RepositoryStrategy.GetOrCreateRepository<IRuneScapeDropDataRepository>();

        var activeRecordResult = repo.GetActive(userId);
        var data = activeRecordResult.ValueOrDefault ?? new RunescapeDropData { UserId = userId, Endpoint = endpoint};

        // Update list reference
        var dropList = data.Drops.ToList();
        data = data with { Drops = dropList };

        if (drop is not null) {
            var lastDrop = dropList.LastOrDefault();
            if (lastDrop is not null && !string.IsNullOrEmpty(lastDrop.Image) && lastDrop.Item is null) {
                drop = drop with { Image = lastDrop.Image };
                dropList.Remove(lastDrop);
            }

            dropList.Add(drop);
        }

        if (!string.IsNullOrEmpty(base64Image)) {
            var imageResult = AddImage(dropList, base64Image);
            if (imageResult.IsFailed) {
                return imageResult;
            }

            drop = imageResult.Value;
        }

        repo.UpdateOrInsert(data);
        return Result.Ok(drop);
    }

    private Result<RunescapeDrop> AddImage(List<RunescapeDrop> drops, string image) {
        RunescapeDrop toUpdate;

        var lastDrop = drops.LastOrDefault();
        if (lastDrop is null || !string.IsNullOrEmpty(lastDrop.Image)) {
            toUpdate = new RunescapeDrop(image);
            drops.Add(toUpdate);
            return Result.Ok(toUpdate);
        }


        toUpdate = lastDrop with { Image = image };
        drops.Remove(lastDrop);
        drops.Add(toUpdate);

        return Result.Ok(toUpdate);
    }

    private Result<DiscordUserId> IsValidEndpoint(EndpointId endpointId) {
        return Result.Ok(DiscordUserId.Empty);
    }

    private async Task<IScheduler> GetScheduler() {
        var schedulers = await _schedulerFactory.GetAllSchedulers();
        return schedulers.FirstOrDefault() ?? await _schedulerFactory.GetScheduler();
    }

    private async Task<Result> ScheduleJob(DiscordUserId userId, RunescapeDrop drop) {
        try {
            var scheduler = await GetScheduler();
            var jobKey = CreateJobKeyByEndpoint(userId);
            var existsTask = scheduler.CheckExists(jobKey);
            var newTrigger = CreateTriggerByDrop(drop);

            if (await existsTask) {
                return await RescheduleJob(scheduler, jobKey, newTrigger);
            }

            Logger.LogInformation("Creating new job: {@key} at {time}", jobKey, newTrigger.StartTimeUtc.TimeOfDay);
            var job = CreateJobWithKey(userId, jobKey);
            await scheduler.ScheduleJob(job, newTrigger);
        } catch (Exception e) {
            var guid = Guid.NewGuid();
            Logger.LogError(e, "Error when scheduling job: {guid}", guid);
            return Result.Fail($"Cannot schedule job. ID of failure: {guid}");
        }

        return Result.Ok();
    }

    private async Task<Result> RescheduleJob(IScheduler scheduler, JobKey jobKey, ITrigger newTrigger) {
        var triggers = await scheduler.GetTriggersOfJob(jobKey);
        if (triggers.Count != 1) {
            return Result.Fail("Job exists, but more then 1 trigger");
        }

        var trigger = triggers.First();
        Logger.LogInformation("Rescheduling job: {@key} at {time}", jobKey, newTrigger.StartTimeUtc.TimeOfDay);
        await scheduler.RescheduleJob(trigger.Key, newTrigger);

        return Result.Ok();
    }

    private IJobDetail CreateJobWithKey(DiscordUserId endpoint, JobKey jobKey) {
        var result = JobBuilder.Create<HandleRunescapeDropJob>()
            .WithIdentity(jobKey)
            .WithDescription("Handling of runescape drop, received through an API request")
            .RequestRecovery()
            .UsingJobData("endpoint", endpoint.Value)
            .Build();

        return result;
    }

    private ITrigger CreateTriggerByDrop(RunescapeDrop drop) {
        var waitTimeInSeconds = drop?.Item?.Name is not null && drop.Source.Name.ToLowerInvariant().Contains("clue") ? _clueWaitTimeInSeconds : _waitTimeInSeconds;

        var trigger = TriggerBuilder.Create()
            .WithIdentity(Guid.NewGuid().ToString())
            .StartAt(DateBuilder.FutureDate(waitTimeInSeconds, IntervalUnit.Second))
            .Build();

        return trigger;
    }

    private JobKey CreateJobKeyByEndpoint(DiscordUserId endpoint) {
        return new JobKey(endpoint.ToString(), "automated-dropper");
    }
}
