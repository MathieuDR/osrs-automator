using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiscordBot.Common.Dtos.Discord;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.Decorators;
using DiscordBot.Common.Models.Enums;
using DiscordBot.Data.Interfaces;
using DiscordBot.Data.Repository;
using DiscordBot.Data.Strategies;
using DiscordBot.Services.Helpers;
using DiscordBot.Services.Interfaces;
using DiscordBot.Services.Models.Enums;
using FluentResults;
using Microsoft.Extensions.Logging;
using Quartz;
using WiseOldManConnector.Helpers;
using WiseOldManConnector.Interfaces;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.Requests;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBot.Services.Services {
    internal class GroupService : RepositoryService, IGroupService {
        private readonly IWiseOldManCompetitionApi _competitionApi;
        private readonly ISchedulerFactory _factory;
        private readonly IWiseOldManGroupApi _groupApi;
        private readonly IOsrsHighscoreService _highscoreService;

        public GroupService(ILogger<GroupService> logger, IRepositoryStrategy repositoryStrategy,
            IOsrsHighscoreService highscoreService,
            IWiseOldManCompetitionApi competitionApi, IWiseOldManGroupApi groupApi,
            ISchedulerFactory factory) :
            base(logger, repositoryStrategy) {
            _highscoreService = highscoreService;
            _competitionApi = competitionApi;
            _groupApi = groupApi;
            _factory = factory;
        }

        public async Task<ItemDecorator<Group>> SetGroupForGuild(GuildUser guildUser, int womGroupId,
            string verificationCode) {
            var group = await _highscoreService.GetGroupById(womGroupId);
            if (group == null) {
                throw new Exception("Group does not exist.");
            }

            var repo = GetRepository<GuildConfigRepository>(guildUser.GuildId);
            var config = repo.GetSingle().Value ?? new GuildConfig(guildUser.GuildId, guildUser.Id);

            config.WomVerificationCode = verificationCode;
            config.WomGroup = group;
            config.WomGroupId = group.Id;

            repo.UpdateOrInsert(config);
            return group.Decorate();
        }

        public async Task SetAutoAdd(GuildUser guildUser, bool autoAdd) {
            var repo = GetRepository<GuildConfigRepository>(guildUser.GuildId);
            var config = GetGroupConfig(guildUser.GuildId);
            if (config.WomGroupId <= 0) {
                throw new Exception("No Wise Old Man set for this server.");
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
                throw new Exception("No configuration for this jobtype set. Perhaps you need to set a channel.");
            }

            var setting = config.AutomatedMessagesConfig.ChannelJobs[jobType];
            setting.IsEnabled = !setting.IsEnabled;
            var repo = GetRepository<GuildConfigRepository>(guild.Id);
            repo.UpdateOrInsert(config);
            return Task.FromResult(setting.IsEnabled);
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
            var temp = await _highscoreService.GetTopDeltasOfGroup(settings.WomGroupId, metricType, period);
            return temp.Decorate(settings.WomGroup.Id, settings.WomGroup.Name);
        }

        public async Task QueueJob(JobType jobType) {
            var schedulers = await _factory.GetAllSchedulers();
            var scheduler = schedulers.FirstOrDefault() ?? await _factory.GetScheduler();

            if (scheduler is null) {
                throw new NullReferenceException("Cannot make a scheduler");
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

            var trigger = TriggerBuilder.Create().StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.Now.AddSeconds(5)))
                .Build();

            await scheduler.ScheduleJob(job, trigger);
        }

        public async Task<Result<ItemDecorator<Competition>>> CreateCompetition(Guild guild, DateTimeOffset start,
            DateTimeOffset end, MetricType metric, CompetitionType competitionType) {
            GuildConfig womConfig = null;
            try {
                womConfig = GetGroupConfig(guild.Id);
            } catch (Exception e) {
                return Result.Fail(new ExceptionalError("Could not retrieve configuration", e));
            }

            CreateCompetitionRequest request;

            var title = new StringBuilder();
            title.Append(womConfig.WomGroup.Name);
            title.Append(" - ");
            title.Append(metric.GetEnumValueNameOrDefault());

            if (competitionType == CompetitionType.Normal) {
                request = new CreateCompetitionRequest(title.ToString(), metric, start.DateTime, end.DateTime,
                    womConfig.WomGroupId, womConfig.WomVerificationCode);
            } else {
                var groupMembers = (await _groupApi.GetMembers(womConfig.WomGroupId)).Data.ToList();
                womConfig.WomGroup.Members = groupMembers;

                var repo = RepositoryStrategy.CreateRepository<GuildConfigRepository>(guild.Id);
                var updateResult = repo.Update(womConfig);


                IEnumerable<CreateCompetitionRequest.Team> teams = new[] {
                    new CreateCompetitionRequest.Team("Placeholder", new[] {groupMembers.FirstOrDefault()?.Username})
                };

                if (competitionType == CompetitionType.CountryTeams) {
                    groupMembers.ForEach(x => x.Country ??= "non affiliated");

                    teams = groupMembers.GroupBy(x => x.Country, x => x.Username)
                        .ToDictionary(g => g.Key, g => g.Select(u => u))
                        .Select(x => new CreateCompetitionRequest.Team(x));
                }

                request = new CreateCompetitionRequest(title.ToString(), metric, start.DateTime, end.DateTime,
                    womConfig.WomGroupId, womConfig.WomVerificationCode, teams);
            }

            try {
                var comp = await _competitionApi.Create(request);
                return Result.Ok(comp.Data.Decorate());
            } catch (Exception e) {
                Logger.LogWarning(e, "Could not create competition");
                return Result.Fail(new ExceptionalError("Could not create competition", e));
            }
        }

        private GuildConfig GetGroupConfig(ulong guildId, bool validate = true) {
            var repo = GetRepository<GuildConfigRepository>(guildId);
            var result = repo.GetSingle().Value;
            if (validate) {
                if (result == null) {
                    throw new Exception("Guild has no configuration. Please set the config");
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
