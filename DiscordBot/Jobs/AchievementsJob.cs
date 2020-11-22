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
using WiseOldManConnector.Models.Output;

namespace DiscordBotFanatic.Jobs {
    public class AchievementsJob : IJob {
        private readonly DiscordSocketClient _discord;
        private readonly ILogService _logService;
        private readonly Mapper _mapper;
        private readonly IOsrsHighscoreService _osrsHighscoreService;
        private readonly IDiscordBotRepository _repository;

        public AchievementsJob(DiscordSocketClient discord, ILogService logService, IDiscordBotRepository repository,
            IOsrsHighscoreService osrsHighscoreService, Mapper mapper) {
            _discord = discord;
            _logService = logService;
            _repository = repository;
            _osrsHighscoreService = osrsHighscoreService;
            _mapper = mapper;
        }

        public async Task Execute(IJobExecutionContext context) {
            _ = _logService.Log("Starting achievements job execution", LogEventLevel.Information, null);

            if (_discord.ConnectionState == ConnectionState.Connected) {
                foreach (SocketGuild discordGuild in _discord.Guilds) {
                    _ = _logService.Log($"Guild: {discordGuild.Name}", LogEventLevel.Information, null);

                    var config = _repository.GetGroupConfig(discordGuild.Id);

                    if (config == null || config.AutomatedMessagesConfig == null ||
                        config.AutomatedMessagesConfig.ChannelJobs == null ||
                        !config.AutomatedMessagesConfig.ChannelJobs.ContainsKey(JobTypes.Achievements)) {
                        // No config set. We don't care.
                        continue;
                    }

                    var settings = config.AutomatedMessagesConfig.ChannelJobs[JobTypes.Achievements];

                    if (!settings.Activated) {
                        // Not activated
                        continue;
                    }

                    var channel = await GetChannel(discordGuild, settings);
                    if (channel == null) {
                        continue;
                    }

                    // Get achievements
                    _ = _logService.Log("Searching achievements", LogEventLevel.Information, null);
                    var achievementsTask = _osrsHighscoreService.GetGroupAchievements(config.WomGroupId);
                    var jobState = _repository.GetAutomatedJobState(config.GuildId);


                    var achievements = (await achievementsTask).OrderBy(a => a.AchievedAt).ToList();

                    // Lookup when last time was printed!
                    int startIndex = 0; 
                    if (jobState == null) {
                        jobState = new AutomatedJobState() {
                            GuildId = config.GuildId,
                            LastPrintedAchievement = new Achievement()
                        };
                    } else {
                        startIndex = achievements.FindIndex(a =>
                            a.PlayerId == jobState.LastPrintedAchievement.PlayerId &&
                            a.Title == jobState.LastPrintedAchievement.Title) + 1;
                    }


                    _ = _logService.Log($"Printing achievements", LogEventLevel.Information, null);

                    int i;
                    for (i = startIndex; i < achievements.Count; i++) {
                        Achievement achievement = achievements[i];

                        var builder = new EmbedBuilder();
                        builder = _mapper.Map(achievement, builder);
                        await channel.SendMessageAsync("", false, builder.Build());
                    }

                    _ = _logService.Log($"Printed {i - startIndex} achievements.", LogEventLevel.Information, null);

                    if (startIndex != achievements.Count-1) {
                        // Updating DB if we printed at least one!
                        jobState.LastPrintedAchievement = achievements.LastOrDefault();
                        _repository.CreateOrUpdateAutomatedJobState(config.GuildId, jobState);
                    }
                }
            } else {
                _ = _logService.Log("Not connected", LogEventLevel.Warning, null);
            }

            _ = _logService.Log("Ending achievements job execution", LogEventLevel.Information, null);
        }

        private bool IsAchievementEqual(Achievement achievement, Achievement toCompare) {
            return achievement.PlayerId == toCompare.PlayerId && achievement.Title == toCompare.Title;
        }

        private Task<ISocketMessageChannel> GetChannel(SocketGuild discordGuild, ChannelJobConfiguration settings) {
            ISocketMessageChannel channel =
                (ISocketMessageChannel) discordGuild.Channels.FirstOrDefault(c => c.Id == settings.ChannelId);

            if (channel == null) {
                _ = _logService.Log("Cannot find channel", LogEventLevel.Warning, null);
                _ = discordGuild.DefaultChannel.SendMessageAsync($"Channel not found for automated job: {JobTypes.Achievements}");
            }

            return Task.FromResult(channel);
        }
    }
}