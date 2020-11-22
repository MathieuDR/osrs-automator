using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discord;
using Discord.WebSocket;
using DiscordBotFanatic.Models.Data;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Repository;
using DiscordBotFanatic.Services.interfaces;
using Quartz;
using Serilog.Events;

namespace DiscordBotFanatic.Jobs {
    public abstract class BaseGuildJob : IJob {
        protected DiscordSocketClient Discord { get; }
        protected ILogService LogService { get; }
        protected IDiscordBotRepository Repository { get; }
        protected Mapper Mapper { get; }
        protected JobTypes JobType { get; }
        protected GroupConfig Configuration { get; private set; }


        protected BaseGuildJob(DiscordSocketClient discord, ILogService logService, IDiscordBotRepository repository, Mapper mapper, JobTypes jobType) {
            Discord = discord;
            LogService = logService;
            Repository = repository;
            Mapper = mapper;
            JobType = jobType;
        }


        public virtual async Task Execute(IJobExecutionContext context) {
             _ = LogService.Log($"Starting {JobType} job execution", LogEventLevel.Information, null);

            if (Discord.ConnectionState == ConnectionState.Connected) {
                foreach (SocketGuild discordGuild in Discord.Guilds) {
                    _ = LogService.Log($"Guild: {discordGuild.Name}, done", LogEventLevel.Information, null);
                    Configuration = Repository.GetGroupConfig(discordGuild.Id);

                    if (Configuration == null || Configuration.AutomatedMessagesConfig == null ||
                        Configuration.AutomatedMessagesConfig.ChannelJobs == null ||
                        !Configuration.AutomatedMessagesConfig.ChannelJobs.ContainsKey(JobTypes.Achievements)) {
                        // No config set. We don't care.
                        continue;
                    }

                    var settings = Configuration.AutomatedMessagesConfig.ChannelJobs[JobTypes.Achievements];

                    if (!settings.Activated) {
                        // Not activated
                        continue;
                    }

                    var channel = await GetChannel(discordGuild, settings);
                    if (channel == null) {
                        continue;
                    }

                    // Do task for guild
                    await ForGuild(discordGuild, channel);

                    _ = LogService.Log($"Guild: {discordGuild.Name}, starting", LogEventLevel.Information, null);
                }
            } else {
                _ = LogService.Log("Not connected", LogEventLevel.Warning, null);
            }

            _ = LogService.Log($"Ending {JobType} job execution", LogEventLevel.Information, null);
        }

        public abstract Task ForGuild(SocketGuild guild, IMessageChannel channel);

        protected Task<ISocketMessageChannel> GetChannel(SocketGuild guild, ChannelJobConfiguration config) {
            ISocketMessageChannel channel =
                (ISocketMessageChannel) guild.Channels.FirstOrDefault(c => c.Id == config.ChannelId);

            if (channel == null) {
                _ = LogService.Log("Cannot find channel", LogEventLevel.Warning, null);
                _ = guild.DefaultChannel.SendMessageAsync($"Channel not found for automated job: {JobTypes.Achievements}");
            }

            return Task.FromResult(channel);
        }
    }
}