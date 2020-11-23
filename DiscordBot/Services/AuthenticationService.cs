using System.Threading.Tasks;
using Discord;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Services {
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