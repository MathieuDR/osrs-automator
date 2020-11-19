using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Data;
using DiscordBotFanatic.Models.Decorators;
using DiscordBotFanatic.Repository;
using DiscordBotFanatic.Services.interfaces;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;
using Player = DiscordBotFanatic.Models.Data.Player;

namespace DiscordBotFanatic.Services {
    public class GroupService : BaseService, IGroupService {
        private readonly IOsrsHighscoreService _highscoreService;
        private readonly IDiscordBotRepository _repository;
 
        public GroupService(ILogService logger, IDiscordBotRepository repository, IOsrsHighscoreService highscoreService) : base(logger) {
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

            _repository.UpdateOrInsertGroupConfig(config);
            return group.Decorate();
        }

        public async Task SetAutoAdd(IGuildUser guildUser, bool autoAdd) {
            GroupConfig config = _repository.GetGroupConfig(guildUser.GuildId);
            if (config == null || config.WomGroupId <= 0) {
                throw new Exception($"No group set for this server.");
            }

            config.AutoAddNewAccounts = autoAdd;

            var _ = Task.Run(() => {
                if (autoAdd) {
                    AddAllPlayersToGroup(guildUser, config);
                }
            });

            _repository.UpdateOrInsertGroupConfig(config);
            await _;
        }

        public Task<Dictionary<string, string>> GetSettingsDictionary(IGuild guild) {
            var settings = _repository.GetGroupConfig(guild.Id);

            if (settings == null) {
                return Task.FromResult(new Dictionary<string, string>());
            }

            return Task.FromResult(settings.ToDictionary());
        }

        public Task<ItemDecorator<Leaderboard>> GetGroupLeaderboard(IGuildUser guildUser) {
            return GetGroupLeaderboard(guildUser, MetricType.Overall, Period.Week);
        }

        public async Task<ItemDecorator<Leaderboard>> GetGroupLeaderboard(IGuildUser guildUser, MetricType metricType, Period period) {
            var settings = _repository.GetGroupConfig(guildUser.GuildId);

            // Check if competition is running
            // Maybe go to competition service? Not sure
            var competition = (await _highscoreService.GetAllCompetitionsForGroup(settings.WomGroupId)).FirstOrDefault(c => c.EndDate >= DateTimeOffset.Now);
            
            if (competition != null) {
                return competition.DecorateLeaderboard();
            }

            // Check for delta gains, overall is standard
           DeltaLeaderboard temp = await _highscoreService.GetTopDeltasOfGroup(settings.WomGroupId, metricType, period);
           return temp.DecorateGeneric(settings.WomGroup);
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