using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using WiseOldManConnector.Models.Output;

namespace DiscordBotFanatic.Services.interfaces {
    public interface IPlayerService {
        /// <summary>
        /// Couples a discord user with an osrs user.
        /// </summary>
        /// <param name="discordUser">Said discord user</param>
        /// <param name="proposedOsrsName">Proposed name</param>
        /// <returns></returns>
        Task<Player> CoupleDiscordGuildUserToOsrsAccount(IGuildUser discordUser, string proposedOsrsName);

        Task<IEnumerable<Player>> GetAllOsrsAccounts(IGuildUser user);
        Task DeleteCoupleOsrsAccountAtIndex(IGuildUser user, int index);
    }
}