using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Decorators;
using DiscordBot.Common.Models.DiscordDtos;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Helpers;
using DiscordBot.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Quartz;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Services.Services {
    public class GroupService : BaseService, IGroupService {
        private readonly IOsrsHighscoreService _highscoreService;
        private readonly ISchedulerFactory _factory;

        public GroupService(ILogger<GroupService> logger, RepositoryStrategy repositoryStrategy, IOsrsHighscoreService highscoreService, ISchedulerFactory factory) :
            base(logger, repositoryStrategy) {
            _highscoreService = highscoreService;
            _factory = factory;
        }

        public async Task<ItemDecorator<Group>> SetGroupForGuild(GuildUser guildUser, int womGroupId, string verificationCode) {
            Group group = await _highscoreService.GetGroupById(womGroupId);
            if (group == null) {
                throw new Exception($"Group does not exist.");
            }
            
            var repo = GetRepository<GuildConfigRepository>(guildUser.GuildId);
            GuildConfig config = repo.GetSingle().Value ?? new GuildConfig(guildUser.GuildId, guildUser.Id);

            config.WomVerificationCode = verificationCode;
            config.WomGroup = group;
            config.WomGroupId = group.Id;

            repo.UpdateOrInsert(config);
            return group.Decorate();
        }

        public async Task SetAutoAdd(GuildUser guildUser, bool autoAdd) {
            var repo = GetRepository<GuildConfigRepository>(guildUser.GuildId);
            GuildConfig config = GetGroupConfig(guildUser.GuildId);
            if (config.WomGroupId <= 0) {
                throw new Exception($"No Wise Old Man set for this server.");
            }

            config.AutoAddNewAccounts = autoAdd;

            var _ = Task.Run(() => {
                if (autoAdd) {
                    AddAllPlayersToGroup(guildUser, config);
                }
            });

            repo.UpdateOrInsert(config);
            await _;
        }

        public Task SetAutomationJobChannel(JobType jobType, GuildUser user, Channel messageChannel) {
            var config = GetGroupConfig(user.GuildId);

            //ChannelJobConfiguration setting;
            if (config.AutomatedMessagesConfig.ChannelJobs.ContainsKey(jobType)) {
                var setting = config.AutomatedMessagesConfig.ChannelJobs[jobType];
                setting.ChannelId = messageChannel.Id;
            } else {
                var setting = new ChannelJobConfiguration(user.GuildId, messageChannel.Id);
                config.AutomatedMessagesConfig.ChannelJobs.Add(jobType, setting);
            }

            var repo = GetRepository<GuildConfigRepository>(user.GuildId);
            repo.UpdateOrInsert(config);
            return Task.CompletedTask;
        }

        public Task<bool> ToggleAutomationJob(JobType jobType, Guild guild) {
            var config = GetGroupConfig(guild.Id);

            //ChannelJobConfiguration setting;
            if (!config.AutomatedMessagesConfig.ChannelJobs.ContainsKey(jobType)) {
                throw new Exception($"No configuration for this jobtype set. Perhaps you need to set a channel.");
            }

            var setting = config.AutomatedMessagesConfig.ChannelJobs[jobType];
            setting.Activated = !setting.Activated;
            var repo = GetRepository<GuildConfigRepository>(guild.Id);
            repo.UpdateOrInsert(config);
            return Task.FromResult(setting.Activated);
        }

        public Task SetActivationAutomationJob(JobType jobType, bool activated) {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, string>> GetSettingsDictionary(Guild guild) {
            var settings = GetGroupConfig(guild.Id, false);

            if (settings == null) {
                return Task.FromResult(new Dictionary<string, string>());
            }

            return Task.FromResult(settings.ToDictionary());
        }

        public Task<ItemDecorator<Leaderboard>> GetGroupLeaderboard(GuildUser guildUser) {
            return GetGroupLeaderboard(guildUser, MetricType.Overall, Period.Week);
        }

        public async Task<ItemDecorator<Leaderboard>> GetGroupLeaderboard(GuildUser guildUser, MetricType metricType,
            Period period) {
            var settings = GetGroupConfig(guildUser.GuildId);

            // Check if competition is running
            // Maybe go to competition service? Not sure
            var emptyCompetition =
                (await _highscoreService.GetAllCompetitionsForGroup(settings.WomGroupId)).FirstOrDefault(c =>
                    c.EndDate >= DateTimeOffset.Now);

            if (emptyCompetition != null) {
                var competition = await _highscoreService.GetCompetition(emptyCompetition.Id);
                return competition.DecorateLeaderboard();
            }
            
            // Check for delta gains, overall is standard
            DeltaLeaderboard temp = await _highscoreService.GetTopDeltasOfGroup(settings.WomGroupId, metricType, period);
            return temp.Decorate(settings.WomGroup.Id, settings.WomGroup.Name);
        }

        public async Task QueueJob(JobType jobType) {
            var schedulers = await _factory.GetAllSchedulers();
            var scheduler = schedulers.FirstOrDefault() ?? await _factory.GetScheduler();

            if (scheduler is null) {
                throw new NullReferenceException($"Cannot make a scheduler");
            }

            var t = typeof(GroupService);
            // var t = jobType switch {
            //     JobType.GroupUpdate => typeof(AutoUpdateGroupJob),
            //     JobType.MonthlyTop => typeof(TopLeaderBoardJob),
            //     JobType.MonthlyTopGains => typeof(MonthlyTopDeltasJob),
            //     _ => throw new ArgumentOutOfRangeException(nameof(jobType), jobType, null)
            // };

            var job = JobBuilder.Create(t)
                .WithIdentity(Guid.NewGuid().ToString())
                .Build();

            var trigger = TriggerBuilder.Create().StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.Now.AddSeconds(5))).Build();
            
            scheduler.ScheduleJob(job, trigger);
        }

        private GuildConfig GetGroupConfig(ulong guildId, bool validate = true) {
            var repo = GetRepository<GuildConfigRepository>(guildId);
            var result = repo.GetSingle().Value;
            if (validate) {
                if (result == null) {
                    throw new Exception($"Guild has no configuration. Please set the config");
                }
            }


            return result;
        }

        private Task AddAllPlayersToGroup(GuildUser guildUser, GuildConfig config) {
            var repo = GetRepository<IPlayerRepository>(guildUser.GuildId);
            var players = repo.GetAll().Value;
            var usernames = new List<string>();

            foreach (var player in players) {
                var membersForPlayer = player.CoupledOsrsAccounts.Select(osrs => osrs.Username).ToList();
                usernames.AddRange(membersForPlayer);
            }

            _highscoreService.AddOsrsAccountToToGroup(config.WomGroupId, config.WomVerificationCode, usernames);

            return Task.CompletedTask;
        }
    }
}