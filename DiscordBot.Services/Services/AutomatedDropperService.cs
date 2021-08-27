using System;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Common.Dtos.Runescape;
using DiscordBot.Common.Models.Data;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Interfaces;
using DiscordBot.Services.Jobs;
using FluentResults;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Util;

namespace DiscordBot.Services.Services {
    internal class AutomatedDropperService : RepositoryService, IAutomatedDropperService {
        private readonly ISchedulerFactory _schedulerFactory;

        public AutomatedDropperService(ILogger<AutomatedDropperService> logger, IRepositoryStrategy repositoryStrategy,
            ISchedulerFactory schedulerFactory) : base(logger, repositoryStrategy) {
            _schedulerFactory = schedulerFactory;
        }

        public async Task<Result> HandleDropRequest(Guid endpoint, RunescapeDrop drop, string base64Image) {
            //Check user Id
            var allowedCheckResult = IsValidEndpoint(endpoint);
            if (allowedCheckResult.IsFailed) {
                Logger.LogInformation("Not allowed endpoint: {endpoint}", endpoint);
                return allowedCheckResult;
            }
            
            //Save to DB
            var saveInformationResult = SaveDropData(endpoint, drop, base64Image);
            if (saveInformationResult.IsFailed) {
                Logger.LogWarning("Could not save drop with endpoint {endpoint}, data: {@drop} and {verb} image", endpoint, drop, string.IsNullOrEmpty(base64Image) ? "without" : "with");
                return saveInformationResult;
            }

            //Schedule job
            var schedulingResult = await ScheduleJob(endpoint, drop);
            if (schedulingResult.IsFailed) {
                Logger.LogWarning("Could not schedule job with id {endpoint}", endpoint);
                return schedulingResult;
            }

            return Result.Ok();
        }

        private Result SaveDropData(Guid endpoint, RunescapeDrop drop, string base64Image) {
            var repo = GetRepository<IRuneScapeDropDataRepository>();

            var activeRecordResult = repo.GetActive(endpoint);
            var data = activeRecordResult.ValueOrDefault ?? new RunescapeDropData();

            // Update
            var dropList = data.Drops.ToList();
            var imageList = data.Images.ToList();
            data = data with { Endpoint = endpoint, Drops = dropList, Images = imageList};
            
            if (drop is not null) {
                data = data with { RecipientUsername = drop.Recipient.Username, RecipientPlayerType = drop.Recipient.PlayerType, Endpoint = endpoint, Drops = dropList, Images = imageList};
                dropList.Add(drop);
            }
            
            if (!base64Image.IsNullOrWhiteSpace()) {
                imageList.Add(base64Image);
            }

            repo.UpdateOrInsert(data);
            return Result.Ok();
        }

        private Result IsValidEndpoint(Guid userId) {
            return Result.Ok();
        }

        private async Task<IScheduler> GetScheduler() {
            var schedulers = await _schedulerFactory.GetAllSchedulers();
            return schedulers.FirstOrDefault() ?? await _schedulerFactory.GetScheduler();
        }

        private async Task<Result> ScheduleJob(Guid endpoint, RunescapeDrop drop) {
            try {
                var scheduler = await GetScheduler();
                var jobKey = CreateJobKeyByEndpoint(endpoint);
                var existsTask = scheduler.CheckExists(jobKey);
                var newTrigger = CreateTriggerByDrop(drop);

                if (await existsTask) {
                    return await RescheduleJob(scheduler, jobKey, newTrigger);
                }

                Logger.LogInformation("Creating new job: {@key} at {time}", jobKey, newTrigger.StartTimeUtc.TimeOfDay);
                var job = CreateJobWithKey(endpoint ,jobKey);
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

        private IJobDetail CreateJobWithKey(Guid endpoint, JobKey jobKey) {
            var result = JobBuilder.Create<HandleRunescapeDropJob>()
                .WithIdentity(jobKey)
                .WithDescription("Handling of runescape drop, received through an API request")
                .RequestRecovery()
                .UsingJobData("endpoint", endpoint)
                .Build();

            return result;
        }

        private ITrigger CreateTriggerByDrop(RunescapeDrop drop) {
            
            var waitTimeInSeconds = drop is not null && drop.Source.Name.ToLowerInvariant().Contains("clue") ? 60 : 2;

            var trigger = TriggerBuilder.Create()
                .WithIdentity(Guid.NewGuid().ToString())
                .StartAt(DateBuilder.FutureDate(waitTimeInSeconds, IntervalUnit.Second))
                .Build();

            return trigger;
        }

        private JobKey CreateJobKeyByEndpoint(Guid endpoint) {
            return new(endpoint.ToString(), "automated-dropper");
        }

        // Job
        // Gather all of username
        // Filter
        // -- Filtered keywords
        // --- Total value/HA
        // --- Check for collection log item
        // if exceeds for a post?
        // --create post
        // --send post to all discord guilds where that username is registered (atm all)
        // remove from db, end Job
    }
}
