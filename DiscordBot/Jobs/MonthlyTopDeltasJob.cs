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
    public class MonthlyTopDeltasJob : BaseGuildJob {
        private readonly IOsrsHighscoreService _osrsHighscoreService;
       
        private readonly Period _period;

        public MonthlyTopDeltasJob(DiscordSocketClient discord, ILogService logService, IDiscordBotRepository repository,
            Mapper mapper, IOsrsHighscoreService osrsHighscoreService) : base(discord,
            logService, repository, mapper, JobType.MonthlyTopGains) {
            _osrsHighscoreService = osrsHighscoreService;
            
            _period = Period.Month;
        }

        public override async Task ForGuild(SocketGuild guild, IMessageChannel channel) {
            _ = LogService.Log("Searching top", LogEventLevel.Information);

            var metrics = MetricTypeCategory.All.GetMetricTypes();

            List<DeltaLeaderboard> tops = new List<DeltaLeaderboard>();
            
            try {
                foreach (var metric in metrics) {   
                    _ = LogService.Log($"Calling for {metric}", LogEventLevel.Information);
                    var top = await _osrsHighscoreService.GetTopDeltasOfGroup(Configuration.WomGroupId, metric, _period);
                    tops.Add(top);
                }
            } catch (Exception e) {
                Console.WriteLine(e);
                await CreateRecovery();
                return;
            }

            var metricMessages = new List<string>();
            foreach (var leaderboard in tops) {
                var message = $"**{leaderboard.MetricType.FriendlyName(true)}** - top gains for {_period}{Environment.NewLine}```";
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
