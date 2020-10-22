using System.Threading.Tasks;
using Discord;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Services {
    public class AuthenticationService :IAuthenticationService {
        public async Task<bool> IsAllowed(IUserGuild guildUser, BotPermissions permissions) {
            throw new System.NotImplementedException();
        }

        public async Task AddPermissions(IUserGuild guildUser, BotPermissions toAdd) {
            throw new System.NotImplementedException();
        }

        public async Task RemovePermissions(IUserGuild guildUser, BotPermissions toRemove) {
            throw new System.NotImplementedException();
        }

        public async Task SetPermissions(IUserGuild guildUser, BotPermissions permissions) {
            throw new System.NotImplementedException();
        }

        public async Task<BotPermissions> TogglePermissions(IUserGuild guildUser, BotPermissions permissions) {
            throw new System.NotImplementedException();
        }
    }
}