using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Discord;
using Discord.WebSocket;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Repository;
using DiscordBotFanatic.Services.interfaces;
using DiscordBotFanatic.Transformers;
using Quartz;
using Serilog.Events;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBotFanatic.Jobs {
    public class TopLeaderBoardJob : BaseGuildJob {
        private readonly IOsrsHighscoreService _osrsHighscoreService;

        public TopLeaderBoardJob(DiscordSocketClient discord, ILogService logService, IDiscordBotRepository repository,
            Mapper mapper, IOsrsHighscoreService osrsHighscoreService) : base(discord,
            logService, repository, mapper, JobType.DailyTop) {
            _osrsHighscoreService = osrsHighscoreService;
        }

        public override async Task ForGuild(SocketGuild guild, IMessageChannel channel) {
            _ = LogService.Log("Searching top", LogEventLevel.Information);

            var metrics = MetricTypeCategory.All.GetMetricTypes();

            List<HighscoreLeaderboard> tops;
            try {
                var tasks = new List<Task<HighscoreLeaderboard>>();
                foreach (var metric in metrics) {
                    _ = LogService.Log($"Calling for {metric}", LogEventLevel.Information);
                    var task = _osrsHighscoreService.GetLeaderboard(Configuration.WomGroupId, metric);
                    tasks.Add(task);
                }
                
                await Task.WhenAll(tasks);
                tops = tasks.Select(x => x.Result).ToList();
            } catch (Exception e) {
                Console.WriteLine(e);
                await CreateRecovery(1);
                return;
            }

            

            // var embeds = new List<Embed>();
            var metricMessages = new List<string>();
            foreach (var leaderboard in tops) {
                _ = LogService.Log($"Making message for {leaderboard.MetricType}", LogEventLevel.Information);
                var message = $"**{leaderboard.MetricType.FriendlyName(true)}** - top{Environment.NewLine}```";
                message += leaderboard.MembersToString(3);
                message += $"```{Environment.NewLine}";
                metricMessages.Add(message);
            }

            var maxSize = 1990;
            var builder = new StringBuilder();
            foreach (var metricMessage in metricMessages) {
                if (builder.Length + metricMessage.Length >= maxSize) {
                    if (builder.Length > 0) {
                        await channel.SendMessageAsync(builder.ToString());
                    }

                    builder = new StringBuilder();
                }

                builder.Append(metricMessage);
            }
            
            // Last message
            if (builder.Length > 0) {
                _ = channel.SendMessageAsync(builder.ToString());
            }
        }
    }
}
