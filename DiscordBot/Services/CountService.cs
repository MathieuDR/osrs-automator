using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using DiscordBotFanatic.Models;
using DiscordBotFanatic.Repository;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Services {
    public class CountService : BaseService, ICounterService {
        private readonly IDiscordBotRepository _repositoryService;

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

        public CountService(ILogService logger, IDiscordBotRepository repositoryService) : base(logger) {
            _repositoryService = repositoryService;
        }


        private UserCountInfo GetOrCreateUserCountInfo(IGuildUser user, IGuildUser requester = null) {
            var result = _repositoryService.GetCountInfoByUserId(user.GuildId, user.Id) ?? 
                         new UserCountInfo(requester??user) {DiscordId = user.Id};

            return result;
        }
    }
}
