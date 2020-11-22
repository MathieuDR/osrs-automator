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
    public class AchievementsJob : BaseGuildJob {
        private readonly IOsrsHighscoreService _osrsHighscoreService;


        public AchievementsJob(DiscordSocketClient discord, ILogService logService, IDiscordBotRepository repository,Mapper mapper,IOsrsHighscoreService osrsHighscoreService) : base(discord, logService, repository, mapper, JobTypes.Achievements) {
            _osrsHighscoreService = osrsHighscoreService;
        }

        public override async Task ForGuild(SocketGuild guild, IMessageChannel channel) {
             _ = LogService.Log("Searching achievements", LogEventLevel.Information, null);
                    var achievementsTask = _osrsHighscoreService.GetGroupAchievements(Configuration.WomGroupId);
                    var jobState = Repository.GetAutomatedJobState(Configuration.GuildId);


                    var achievements = (await achievementsTask).OrderBy(a => a.AchievedAt).ToList();

                    // Lookup when last time was printed!
                    int startIndex = 0;
                    if (jobState == null) {
                        jobState = new AutomatedJobState() {
                            GuildId = Configuration.GuildId,
                            LastPrintedAchievement = new Achievement()
                        };
                    } else {
                        startIndex = achievements.FindIndex(a =>
                            a.PlayerId == jobState.LastPrintedAchievement.PlayerId &&
                            a.Title == jobState.LastPrintedAchievement.Title) + 1;
                    }


                    _ = LogService.Log($"Printing achievements", LogEventLevel.Information, null);

                    int i;
                    for (i = startIndex; i < achievements.Count; i++) {
                        Achievement achievement = achievements[i];

                        var builder = new EmbedBuilder();
                        builder = Mapper.Map(achievement, builder);
                        await channel.SendMessageAsync("", false, builder.Build());
                    }

                    _ = LogService.Log($"Printed {i - startIndex} achievements.", LogEventLevel.Information, null);

                    if (startIndex != achievements.Count - 1) {
                        // Updating DB if we printed at least one!
                        jobState.LastPrintedAchievement = achievements.LastOrDefault();
                        Repository.CreateOrUpdateAutomatedJobState(Configuration.GuildId, jobState);
                    }
        }
        
        private bool IsAchievementEqual(Achievement achievement, Achievement toCompare) {
            return achievement.PlayerId == toCompare.PlayerId && achievement.Title == toCompare.Title;
        }
    }
}