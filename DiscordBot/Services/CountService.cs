using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordBot.Models.Data;
using DiscordBot.Repository;
using DiscordBot.Services.interfaces;

namespace DiscordBot.Services {
    public class CountService : BaseService, ICounterService {
        private readonly IDiscordBotRepository _repositoryService;

        public CountService(ILogService logger, IDiscordBotRepository repositoryService) : base(logger) {
            _repositoryService = repositoryService;
        }


        public int TotalCount(IGuildUser user) {
            var count = GetOrCreateUserCountInfo(user);
            return count.CurrentCount;
        }

        public int Count(IGuildUser user, IGuildUser requester, int additive, string reason) {
            var count = GetOrCreateUserCountInfo(user, requester);
            var newCount = new Count(requester, additive, reason);
            count.CountHistory.Add(newCount);

            _repositoryService.UpdateOrInsertUserCountInfoForGuid(user.GuildId, count);
            return count.CurrentCount;
        }

        public UserCountInfo GetCountInfo(IGuildUser user) {
            return GetOrCreateUserCountInfo(user);
        }

        public List<UserCountInfo> TopCounts(SocketGuild contextGuild, int quantity) {
            var all = _repositoryService.GetAllUserCountInfos(contextGuild.Id).ToList();
            return all.OrderByDescending(c => c.CurrentCount).Take(quantity).ToList();
        }

       

        private UserCountInfo GetOrCreateUserCountInfo(IGuildUser user, IGuildUser requester = null) {
            var result = _repositoryService.GetCountInfoByUserId(user.GuildId, user.Id) ?? 
                         new UserCountInfo(requester??user) {DiscordId = user.Id};

            return result;
        }
        
        public Task<bool> SetChannelForCounts(IGuildUser creator, IChannel outputChannel) {
            var config = GetGroupConfig(creator.GuildId);
            config.CountConfig ??= new CountConfig();
            
            config.CountConfig.OutputChannelId = outputChannel.Id;
            _repositoryService.CreateOrUpdateGroupConfig(config);
            return Task.FromResult(true);
        }

        public Task<bool> CreateTreshold(IGuildUser creator, int count, string name, IRole role = null) {
            var config = GetGroupConigWithValidCountConfig(creator.GuildId);
            var toAdd = new CountTreshold();
            toAdd.name = string.IsNullOrEmpty(name) ? "unnamed" : name;
            toAdd.Treshold = count;
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

        public Task<IReadOnlyList<CountTreshold>> GetTresholds(ulong guildId) {
            var config = GetGroupConigWithValidCountConfig(guildId);
            return Task.FromResult(config.CountConfig.Tresholds);
        }

        public Task<ulong> GetChannelForGuild(ulong guildId) {
            var config = GetGroupConigWithValidCountConfig(guildId);
            return Task.FromResult(config.CountConfig.OutputChannelId);
        }

        private GroupConfig GetGroupConigWithValidCountConfig(ulong guildId) {
            GroupConfig config = GetGroupConfig(guildId);

            if (config.CountConfig is null) {
                throw new Exception($"Please set up an output channel first.");
            }

            return config;
        }
        
        private GroupConfig GetGroupConfig(ulong guildId, bool validate = true) {
            var result = _repositoryService.GetGroupConfig(guildId);
            
            if (result == null) {
                throw new Exception($"Guild has no configuration. Please set the config");
            }
            
            return result;
        }

    }
}
