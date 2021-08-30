using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.DiscordDtos;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Interfaces;
using DiscordBot.Services.Jobs;
using FluentResults;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Jobs {
    public class TopLeaderBoardJob : ConfigurableGuildJob {
        private readonly IOsrsHighscoreService _osrsHighscoreService;

        public TopLeaderBoardJob(ILogger<TopLeaderBoardJob> logger, IDiscordService discordService, IRepositoryStrategy repositoryStrategy,
            IOsrsHighscoreService osrsHighscoreService) : base(logger, discordService, JobType.MonthlyTopGains, repositoryStrategy) {
            _osrsHighscoreService = osrsHighscoreService;
        }

        protected override async Task<Result> DoWorkForGuildWithContext(Guild guild, GuildConfig guildConfig, ChannelJobConfiguration configuration) {
            var metrics = MetricTypeCategory.Queryable.GetMetricTypes();
            var tops = new List<HighscoreLeaderboard>();

            try {
                foreach (var metric in metrics) {
                    Logger.LogInformation("Getting top deltas for {metric}", metric);
                    var top = await _osrsHighscoreService.GetLeaderboard(guildConfig.WomGroupId, metric);
                    tops.Add(top);
                }
            } catch (Exception e) {
                Logger.LogError(e, "Error while getting top delta's");
                return Result.Fail(new ExceptionalError(e));
            }

            return await DiscordService.MessageLeaderboards(configuration.ChannelId, tops);
        }
    }
}
