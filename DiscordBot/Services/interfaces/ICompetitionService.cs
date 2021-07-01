using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using DiscordBot.Models.Decorators;
using WiseOldManConnector.Models.Output;

namespace DiscordBot.Services.interfaces {
    public interface ICompetitionService {
        Task<ItemDecorator<Competition>> SetCurrentCompetition(IGuildUser guildUser, int id);
        Task<ItemDecorator<Competition>> SetCurrentCompetition(IGuildUser guildUser, string name);

        Task<ItemDecorator<Competition>> ViewCurrentCompetition(IGuild guild);
        Task<IEnumerable<ItemDecorator<Competition>>> ViewCompetitionsForGroup(IGuildUser guildUser);
        Task<IEnumerable<ItemDecorator<Competition>>> ViewAllCompetitionsForGroup(IGuildUser guildUser);
    }
}