using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Discord;
using Discord.WebSocket;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Data.Interfaces;
using DiscordBot.Services.Interfaces;
using Serilog.Events;
using WiseOldManConnector.Models.Output;

namespace DiscordBot.Jobs {
    public class AchievementsJob : BaseGuildJob {
        private readonly IOsrsHighscoreService _osrsHighscoreService;


        public AchievementsJob(DiscordSocketClient discord, ILogService logService, IDiscordBotRepository repository,
            Mapper mapper, IOsrsHighscoreService osrsHighscoreService) : base(discord, logService, repository, mapper,
            JobType.Achievements) {
            _osrsHighscoreService = osrsHighscoreService;
        }

        public override async Task ForGuild(SocketGuild guild, IMessageChannel channel) {
            _ = LogService.Log("Searching achievements", LogEventLevel.Information);
            var achievementsTask = _osrsHighscoreService.GetGroupAchievements(Configuration.WomGroupId);
            var jobState = Repository.GetAutomatedJobState(Configuration.GuildId);


            var achievements = (await achievementsTask).OrderBy(a => a.AchievedAt).ToList();

            // Lookup when last time was printed!
            var startIndex = 0;
            if (jobState == null) {
                jobState = new AutomatedJobState {
                    GuildId = Configuration.GuildId,
                    LastPrintedAchievement = new Achievement()
                };
            } else {
                startIndex = achievements.FindIndex(a =>
                    a.PlayerId == jobState.LastPrintedAchievement.PlayerId &&
                    a.Title == jobState.LastPrintedAchievement.Title) + 1;
            }


            _ = LogService.Log("Printing achievements", LogEventLevel.Information);

            int i;
            for (i = startIndex; i < achievements.Count; i++) {
                var achievement = achievements[i];

                var builder = new EmbedBuilder();
                builder = Mapper.Map(achievement, builder);
                await channel.SendMessageAsync("", false, builder.Build());
            }

            var totalPrinted = i - startIndex;
            _ = LogService.Log($"Printed {totalPrinted} achievements.", LogEventLevel.Information);

            if (totalPrinted > 0) {
                // Updating DB if we printed at least one!
                jobState.LastPrintedAchievement = achievements.LastOrDefault();
                Repository.CreateOrUpdateAutomatedJobState(Configuration.GuildId, jobState);
            }
        }
    }
}
