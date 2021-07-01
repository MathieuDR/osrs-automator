using System.Threading.Tasks;
using Discord;
using DiscordBot.Models.Enums;

namespace DiscordBot.Services.interfaces {
    public interface IAuthenticationService {
        public Task<bool> IsAllowed(IUserGuild guildUser, BotPermissions permissions);
        public Task AddPermissions(IUserGuild guildUser, BotPermissions toAdd);
        public Task RemovePermissions(IUserGuild guildUser, BotPermissions toRemove);
        public Task SetPermissions(IUserGuild guildUser, BotPermissions permissions);
        public Task<BotPermissions> TogglePermissions(IUserGuild guildUser, BotPermissions permissions);
    }
}