using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discord;
using Discord.WebSocket;
using DiscordBot.Models.Data;
using DiscordBot.Models.Enums;
using DiscordBot.Repository;
using DiscordBot.Services.interfaces;
using Quartz;
using Serilog.Events;

namespace DiscordBot.Jobs {
    public abstract class BaseGuildJob : IJob {
        protected BaseGuildJob(DiscordSocketClient discord, ILogService logService, IDiscordBotRepository repository,
            Mapper mapper, JobType jobType) {
            Discord = discord;
            LogService = logService;
            Repository = repository;
            Mapper = mapper;
            JobType = jobType;
        }

        protected DiscordSocketClient Discord { get; }
        protected ILogService LogService { get; }
        protected IDiscordBotRepository Repository { get; }
        protected Mapper Mapper { get; }
        protected JobType JobType { get; }
        protected GroupConfig Configuration { get; private set; }
        public IScheduler Scheduler { get; set; }
        
        public IJobDetail Job{ get; set; }

       

        protected async Task CreateRecovery(int minutes = 25) {
            Console.WriteLine($"Stopping execution");
                
            var Trigger = TriggerBuilder.Create()
                .WithIdentity("recoverTrigger", "recovery")
                .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddMinutes(minutes)))
                .Build();
          
            await Scheduler.ScheduleJob(Job, Trigger);
        }

        public virtual async Task Execute(IJobExecutionContext context) {
            _ = LogService.Log($"Starting {JobType} job execution", LogEventLevel.Information);
            Scheduler = context.Scheduler;
            Job = context.JobDetail.GetJobBuilder().WithIdentity(Guid.NewGuid().ToString(),"recovery").Build();

            if (Discord.ConnectionState == ConnectionState.Connected) {
                foreach (SocketGuild discordGuild in Discord.Guilds) {
                    _ = LogService.Log($"Guild: {discordGuild.Name}, starting", LogEventLevel.Information);
                    Configuration = Repository.GetGroupConfig(discordGuild.Id);

                    if (Configuration == null || Configuration.AutomatedMessagesConfig == null ||
                        Configuration.AutomatedMessagesConfig.ChannelJobs == null ||
                        !Configuration.AutomatedMessagesConfig.ChannelJobs.ContainsKey(JobType)) {
                        // No config set. We don't care.
                        continue;
                    }

                    var settings = Configuration.AutomatedMessagesConfig.ChannelJobs[JobType];

                    if (!settings.Activated) {
                        // Not activated
                        continue;
                    }

                    var channel = await GetChannel(discordGuild, settings);
                    if (channel == null) {
                        _ = discordGuild.DefaultChannel.SendMessageAsync($"No channel set for automated job: {JobType}");
                        continue;
                    }

                    // Do task for guild
                    await ForGuild(discordGuild, channel);

                    _ = LogService.Log($"Guild: {discordGuild.Name}, done", LogEventLevel.Information);
                }
            } else {
                _ = LogService.Log("Not connected", LogEventLevel.Warning);
            }

            _ = LogService.Log($"Ending {JobType} job execution", LogEventLevel.Information);
        }

        public abstract Task ForGuild(SocketGuild guild, IMessageChannel channel);

        protected Task<ISocketMessageChannel> GetChannel(SocketGuild guild, ChannelJobConfiguration config) {
            ISocketMessageChannel channel =
                (ISocketMessageChannel) guild.Channels.FirstOrDefault(c => c.Id == config.ChannelId);

            if (channel == null) {
                _ = LogService.Log("Cannot find channel", LogEventLevel.Warning);
                _ = guild.DefaultChannel.SendMessageAsync($"Channel not found for automated job: {JobType}");
            }

            return Task.FromResult(channel);
        }
    }
}