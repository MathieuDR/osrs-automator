using System.Threading.Tasks;
using Discord;
using WiseOldManConnector.Models.Output;

namespace DiscordBotFanatic.Services.interfaces {
    public interface IGroupService {
        public Task<Group> SetGroupForGuild(IGuildUser guildUser, int womGroupId, string verificationCode);
        Task SetAutoAdd(IGuildUser guildUser, bool autoAdd);
    }
}