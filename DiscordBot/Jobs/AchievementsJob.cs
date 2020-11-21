using System;
using System.Collections.Generic;
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
        private readonly ILogService _service;
        private readonly IDiscordBotRepository _repository;
        private readonly IGroupService _groupService;
        private readonly IOsrsHighscoreService _osrsHighscoreService;
        private readonly Mapper _mapper;

        public AchievementsJob(DiscordSocketClient discord, ILogService service, IDiscordBotRepository repository, IOsrsHighscoreService osrsHighscoreService, Mapper mapper) {
            _discord = discord;
            _service = service;
            _repository = repository;
            _osrsHighscoreService = osrsHighscoreService;
            _mapper = mapper;
        }

        public async Task Execute(IJobExecutionContext context) {
            _ = _service.Log("Starting achievements job execution", LogEventLevel.Information, null);

            if (_discord.ConnectionState == ConnectionState.Connected) {
                foreach (SocketGuild discordGuild in _discord.Guilds) {
                    _ = _service.Log($"Guild: {discordGuild.Name}", LogEventLevel.Information, null);

                    var config = _repository.GetGroupConfig(discordGuild.Id);

                    if (config == null || config.AutomatedMessagesConfig == null || config.AutomatedMessagesConfig.ChannelJobs == null || !config.AutomatedMessagesConfig.ChannelJobs.ContainsKey(JobTypes.Achievements)) {
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
                    _ = _service.Log("Searching achievements", LogEventLevel.Information, null);
                    var achievements = await _osrsHighscoreService.GetGroupAchievements(config.WomGroupId);

                    // Lookup when last time was printed!
                    List<Achievement> toPrintAchievements = context.PreviousFireTimeUtc.HasValue ? achievements.Where(a => a.AchievedAt != null && a.AchievedAt.Value >= context.PreviousFireTimeUtc.Value).ToList() : achievements.ToList();
                    _ = _service.Log($"Printing {toPrintAchievements.Count} achievements", LogEventLevel.Information, null);

                    foreach (Achievement achievement in toPrintAchievements) {
                        var builder = new EmbedBuilder();
                        builder = _mapper.Map(achievement, builder);
                        _ = channel.SendMessageAsync("", false, builder.Build());
                    }
                }
            } else {
                _ = _service.Log("Not connected", LogEventLevel.Warning, null);
            }

            
        }

        private async Task<ISocketMessageChannel> GetChannel(SocketGuild discordGuild, ChannelJobConfiguration settings) {
            ISocketMessageChannel channel = (ISocketMessageChannel) discordGuild.Channels.FirstOrDefault(c => c.Id == settings.ChannelId);

            if (channel == null) {
                _ = _service.Log("Cannot find channel", LogEventLevel.Warning, null);
                _ = discordGuild.DefaultChannel.SendMessageAsync($"Channel not found for automated job: {JobTypes.Achievements}");
            }

            return channel;
        }
    }
}