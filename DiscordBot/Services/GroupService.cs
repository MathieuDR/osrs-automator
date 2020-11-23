using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Data;
using DiscordBotFanatic.Models.Decorators;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Repository;
using DiscordBotFanatic.Services.interfaces;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBotFanatic.Services {
    public class GroupService : BaseService, IGroupService {
        private readonly IOsrsHighscoreService _highscoreService;
        private readonly IDiscordBotRepository _repository;

        public GroupService(ILogService logger, IDiscordBotRepository repository, IOsrsHighscoreService highscoreService) :
            base(logger) {
            _repository = repository;
            _highscoreService = highscoreService;
        }

        public async Task<ItemDecorator<Group>> SetGroupForGuild(IGuildUser guildUser, int womGroupId, string verificationCode) {
            Group group = await _highscoreService.GetGroupById(womGroupId);
            if (group == null) {
                throw new Exception($"Group does not exist.");
            }

            GroupConfig config = _repository.GetGroupConfig(guildUser.GuildId) ?? new GroupConfig(guildUser);

            config.WomVerificationCode = verificationCode;
            config.WomGroup = group;
            config.WomGroupId = group.Id;

            _repository.CreateOrUpdateGroupConfig(config);
            return group.Decorate();
        }

        public async Task SetAutoAdd(IGuildUser guildUser, bool autoAdd) {
            GroupConfig config = GetGroupConfig(guildUser.GuildId);
            if (config.WomGroupId <= 0) {
                throw new Exception($"No Wise Old Man set for this server.");
            }

            config.AutoAddNewAccounts = autoAdd;

            var _ = Task.Run(() => {
                if (autoAdd) {
                    AddAllPlayersToGroup(guildUser, config);
                }
            });

            _repository.CreateOrUpdateGroupConfig(config);
            await _;
        }

        public Task SetAutomationJobChannel(JobType jobType, IGuildUser user, IMessageChannel messageChannel) {
            var config = GetGroupConfig(user.GuildId);

            //ChannelJobConfiguration setting;
            if (config.AutomatedMessagesConfig.ChannelJobs.ContainsKey(jobType)) {
                var setting = config.AutomatedMessagesConfig.ChannelJobs[jobType];
                setting.ChannelId = messageChannel.Id;
            } else {
                var setting = new ChannelJobConfiguration(user.GuildId, messageChannel.Id);
                config.AutomatedMessagesConfig.ChannelJobs.Add(jobType, setting);
            }

            _repository.CreateOrUpdateGroupConfig(config);
            return Task.CompletedTask;
        }

        public Task<bool> ToggleAutomationJob(JobType jobType, IGuild guild) {
            var config = GetGroupConfig(guild.Id);

            //ChannelJobConfiguration setting;
            if (!config.AutomatedMessagesConfig.ChannelJobs.ContainsKey(jobType)) {
                throw new Exception($"No configuration for this jobtype set. Perhaps you need to set a channel.");
            }

            var setting = config.AutomatedMessagesConfig.ChannelJobs[jobType];
            setting.Activated = !setting.Activated;
            _repository.CreateOrUpdateGroupConfig(config);
            return Task.FromResult(setting.Activated);
        }

        public Task SetActivationAutomationJob(JobType jobType, bool activated) {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, string>> GetSettingsDictionary(IGuild guild) {
            var settings = GetGroupConfig(guild.Id, false);

            if (settings == null) {
                return Task.FromResult(new Dictionary<string, string>());
            }

            return Task.FromResult(settings.ToDictionary());
        }

        public Task<ItemDecorator<Leaderboard>> GetGroupLeaderboard(IGuildUser guildUser) {
            return GetGroupLeaderboard(guildUser, MetricType.Overall, Period.Week);
        }

        public async Task<ItemDecorator<Leaderboard>> GetGroupLeaderboard(IGuildUser guildUser, MetricType metricType,
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
            return temp.DecorateGeneric(settings.WomGroup);
        }

        private GroupConfig GetGroupConfig(ulong guildId, bool validate = true) {
            var result = _repository.GetGroupConfig(guildId);
            if (validate) {
                if (result == null) {
                    throw new Exception($"Guild has no configuration. Please set the config");
                }
            }


            return result;
        }

        private Task AddAllPlayersToGroup(IGuildUser guildUser, GroupConfig config) {
            var players = _repository.GetAllPlayersForGuild(guildUser.GuildId).ToList();
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