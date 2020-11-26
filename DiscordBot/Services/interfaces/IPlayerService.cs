using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using DiscordBotFanatic.Models.Decorators;
using WiseOldManConnector.Models.Output;

namespace DiscordBotFanatic.Services.interfaces {
    public interface IPlayerService {
        /// <summary>
        /// Couples a discord user with an osrs user.
        /// </summary>
        /// <param name="discordUser">Said discord user</param>
        /// <param name="proposedOsrsName">Proposed name</param>
        /// <returns></returns>
        Task<ItemDecorator<Player>> CoupleDiscordGuildUserToOsrsAccount(IGuildUser discordUser, string proposedOsrsName);

        Task<IEnumerable<ItemDecorator<Player>>> GetAllOsrsAccounts(IGuildUser user);
        Task DeleteCoupledOsrsAccount(IGuildUser user, int id);
        Task<string> SetDefaultAccount(IGuildUser user, Player player);
        Task<string> GetDefaultAccountUserName(IGuildUser user);
    }
}