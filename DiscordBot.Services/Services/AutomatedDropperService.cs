using System;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Common.Dtos.Runescape;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Interfaces;
using DiscordBot.Services.Jobs;
using FluentResults;
using Microsoft.Extensions.Logging;
using Quartz;

namespace DiscordBot.Services.Services {
    internal class AutomatedDropperService : RepositoryService, IAutomatedDropperService {
        private readonly ISchedulerFactory _schedulerFactory;

        public AutomatedDropperService(ILogger<AutomatedDropperService> logger, IRepositoryStrategy repositoryStrategy,
            ISchedulerFactory schedulerFactory) : base(logger, repositoryStrategy) {
            _schedulerFactory = schedulerFactory;
        }

        public async Task<Result> HandleDropRequest(RunescapeDrop drop, string base64Image) {
            var schedulingResult = await ScheduleJob(drop);
            if (schedulingResult.IsFailed) {
                return schedulingResult;
            }

            return Result.Ok();

            // Save to DB
        }

        private async Task<IScheduler> GetScheduler() {
            var schedulers = await _schedulerFactory.GetAllSchedulers();
            return schedulers.FirstOrDefault() ?? await _schedulerFactory.GetScheduler();
        }

        private async Task<Result> ScheduleJob(RunescapeDrop drop) {
            try {
                var scheduler = await GetScheduler();
                var jobKey = CreateJobKeyByRecipient(drop.Recipient.Username);
                var existsTask = scheduler.CheckExists(jobKey);
                var newTrigger = CreateTriggerByDrop(drop);

                if (await existsTask) {
                    return await RescheduleJob(scheduler, jobKey, newTrigger);
                }

                Logger.LogInformation("Creating new job: {@key} at {time}", jobKey, newTrigger.StartTimeUtc.TimeOfDay);
                var job = CreateJobWithKey(drop.Recipient.Username, jobKey);
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

        private IJobDetail CreateJobWithKey(string user, JobKey jobKey) {
            var result = JobBuilder.Create<HandleRunescapeDropJob>()
                .WithIdentity(jobKey)
                .WithDescription("Handling of runescape drop, received through an API request")
                .RequestRecovery()
                .UsingJobData("user", user)
                .Build();

            return result;
        }

        private ITrigger CreateTriggerByDrop(RunescapeDrop drop) {
            var waitTimeInSeconds = drop.Source.Name.ToLowerInvariant().Contains("clue") ? 60 : 2;

            var trigger = TriggerBuilder.Create()
                .WithIdentity(Guid.NewGuid().ToString())
                .StartAt(DateBuilder.FutureDate(waitTimeInSeconds, IntervalUnit.Second))
                .Build();

            return trigger;
        }

        private JobKey CreateJobKeyByRecipient(string recipientUsername) {
            return new(recipientUsername, "automated-dropper");
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
