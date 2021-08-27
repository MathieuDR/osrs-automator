using System.Collections.Generic;
using System.Threading.Tasks;
using DiscordBot.Common.Models.Decorators;
using DiscordBot.Common.Models.DiscordDtos;
using WiseOldManConnector.Models.Output;

namespace DiscordBot.Services.Interfaces {
    public interface IPlayerService {
        /// <summary>
        ///     Couples a discord user with an osrs user.
        /// </summary>
        /// <param name="user">Said discord user</param>
        /// <param name="proposedOsrsName">Proposed name</param>
        /// <returns></returns>
        Task<ItemDecorator<Player>> CoupleDiscordGuildUserToOsrsAccount(GuildUser user, string proposedOsrsName);

        Task<IEnumerable<ItemDecorator<Player>>> GetAllOsrsAccounts(GuildUser user);
        Task DeleteCoupledOsrsAccount(GuildUser user, int id);
        Task<string> SetDefaultAccount(GuildUser user, Player player);
        Task<string> GetDefaultOsrsDisplayName(GuildUser user);

        Task<string> GetUserNickname(GuildUser user, out bool isOsrsAccount);

        //Task<bool> HasSetUsername(IGuildUser user);
        Task<string> SetUserName(GuildUser user, string name);

        /// <summary>
        ///     Preferably use with old username where possible
        /// </summary>
        /// <param name="womAccountId">Account Id to query</param>
        /// <param name="newName">New name</param>
        /// <returns></returns>
        Task<NameChange> RequestNameChange(int womAccountId, string newName);

        Task<NameChange> RequestNameChange(string oldUsername, string newName);
    }
}
