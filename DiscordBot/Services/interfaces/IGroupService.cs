using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using DiscordBotFanatic.Models.Decorators;
using DiscordBotFanatic.Models.Enums;
using WiseOldManConnector.Models.Output;
using WiseOldManConnector.Models.WiseOldMan.Enums;

namespace DiscordBotFanatic.Services.interfaces {
    public interface IGroupService {
        public Task<ItemDecorator<Group>> SetGroupForGuild(IGuildUser guildUser, int womGroupId, string verificationCode);
        Task SetAutoAdd(IGuildUser guildUser, bool autoAdd);
        Task SetAutomationJobChannel(JobTypes jobType, IGuildUser user, IMessageChannel messageChannel);
        Task<bool> ToggleAutomationJob(JobTypes jobType, IGuild guild);
        Task SetActivationAutomationJob(JobTypes jobType, bool activated);
        Task<Dictionary<string, string>> GetSettingsDictionary(IGuild guild);
        Task<ItemDecorator<Leaderboard>> GetGroupLeaderboard(IGuildUser guildUser);
        Task<ItemDecorator<Leaderboard>> GetGroupLeaderboard(IGuildUser guildUser, MetricType metric, Period period);
    }
}