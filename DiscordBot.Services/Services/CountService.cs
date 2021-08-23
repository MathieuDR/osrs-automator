using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordBot.Common.Models.Data;
using DiscordBot.Common.Models.DiscordDtos;
using DiscordBot.Data.Repository;
using DiscordBot.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace DiscordBot.Services.Services {
    public class CountService : BaseService, ICounterService {
        private readonly IDiscordBotRepository _repositoryService;

        public CountService(ILogger<CountService> logger, IDiscordBotRepository repositoryService) : base(logger) {
            _repositoryService = repositoryService;
        }


        public int TotalCount(GuildUser user) {
            var count = GetOrCreateUserCountInfo(user);
            return count.CurrentCount;
        }

        public int Count(GuildUser user, GuildUser requester, int additive, string reason) {
            var count = GetOrCreateUserCountInfo(user, requester);
            var newCount = new Count(requester.Id, requester.Username, additive, reason);
            count.CountHistory.Add(newCount);

            _repositoryService.UpdateOrInsertUserCountInfoForGuid(user.GuildId, count);
            return count.CurrentCount;
        }

        public UserCountInfo GetCountInfo(GuildUser user) {
            return GetOrCreateUserCountInfo(user);
        }

        public List<UserCountInfo> TopCounts(Guild guild, int quantity) {
            var all = _repositoryService.GetAllUserCountInfos(guild.Id).ToList();
            return all.OrderByDescending(c => c.CurrentCount).Take(quantity).ToList();
        }

       

        private UserCountInfo GetOrCreateUserCountInfo(GuildUser user, GuildUser requester = null) {
            var result = _repositoryService.GetCountInfoByUserId(user.GuildId, user.Id) ??
                         new UserCountInfo(requester?.Id ?? user.Id) {DiscordId = user.Id};

            return result;
        }
        
        public Task<bool> SetChannelForCounts(GuildUser creator, Channel outputChannel) {
            var config = GetGroupConfig(creator.GuildId);
            config.CountConfig ??= new CountConfig();
            
            config.CountConfig.OutputChannelId = outputChannel.Id;
            _repositoryService.CreateOrUpdateGroupConfig(config);
            return Task.FromResult(true);
        }

        public Task<bool> CreateThreshold(GuildUser creator, int count, string name, Role role = null) {
            var config = GetGroupConigWithValidCountConfig(creator.GuildId);
            var toAdd = new CountThreshold();
            toAdd.Name = string.IsNullOrEmpty(name) ? "unnamed" : name;
            toAdd.Threshold = count;
            toAdd.GivenRoleId = role?.Id;
            toAdd.CreatorId = creator.Id;
            toAdd.CreatorUsername = creator.Username;

            var result = config.CountConfig.AddTreshold(toAdd);
            
            if (result) {
                _repositoryService.CreateOrUpdateGroupConfig(config);
            }

            return Task.FromResult(result);
        }

        public Task<bool> RemoveCount(ulong guildId, int index) {
            var config = GetGroupConigWithValidCountConfig(guildId);
            var result = config.CountConfig.RemoveAtIndex(index);

            if (result) {
                _repositoryService.CreateOrUpdateGroupConfig(config);
            }

            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<CountThreshold>> GetThresholds(ulong guildId) {
            var config = GetGroupConigWithValidCountConfig(guildId);
            return Task.FromResult(config.CountConfig.Tresholds);
        }

        public Task<ulong> GetChannelForGuild(ulong guildId) {
            var config = GetGroupConigWithValidCountConfig(guildId);
            return Task.FromResult(config.CountConfig.OutputChannelId);
        }

        private GuildConfig GetGroupConigWithValidCountConfig(ulong guildId) {
            GuildConfig config = GetGroupConfig(guildId);

            if (config.CountConfig is null) {
                throw new Exception($"Please set up an output channel first.");
            }

            return config;
        }
        
        private GuildConfig GetGroupConfig(ulong guildId, bool validate = true) {
            var result = _repositoryService.GetGroupConfig(guildId);
            
            if (result == null) {
                throw new Exception($"Guild has no configuration. Please set the config");
            }
            
            return result;
        }

    }
}
