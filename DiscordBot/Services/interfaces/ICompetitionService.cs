using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using WiseOldManConnector.Models.Output;

namespace DiscordBotFanatic.Services.interfaces {
    public interface ICompetitionService {
        Task<Competition> SetCurrentCompetition(IGuildUser guildUser, int id);
        Task<Competition> SetCurrentCompetition(IGuildUser guildUser, string name);

        Task<Competition> ViewCurrentCompetition(IGuild guild);
        Task<IEnumerable<Competition>> ViewCompetitionsForGroup(IGuildUser guildUser);
        Task<IEnumerable<Competition>> ViewAllCompetitionsForGroup(IGuildUser guildUser);
    }
}