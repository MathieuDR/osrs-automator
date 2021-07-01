using System.Threading.Tasks;
using Discord;
using DiscordBot.Models.Enums;
using DiscordBot.Services.interfaces;

namespace DiscordBot.Services {
    public class AuthenticationService : IAuthenticationService {
        public Task<bool> IsAllowed(IUserGuild guildUser, BotPermissions permissions) {
            throw new System.NotImplementedException();
        }

        public Task AddPermissions(IUserGuild guildUser, BotPermissions toAdd) {
            throw new System.NotImplementedException();
        }

        public Task RemovePermissions(IUserGuild guildUser, BotPermissions toRemove) {
            throw new System.NotImplementedException();
        }

        public Task SetPermissions(IUserGuild guildUser, BotPermissions permissions) {
            throw new System.NotImplementedException();
        }

        public Task<BotPermissions> TogglePermissions(IUserGuild guildUser, BotPermissions permissions) {
            throw new System.NotImplementedException();
        }
    }
}