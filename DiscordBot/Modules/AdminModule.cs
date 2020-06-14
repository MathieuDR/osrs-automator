using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using DiscordBotFanatic.Models.Enums;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Modules {

    [Name("Administrator")]
    [Group("admin")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public class AdminModule : ModuleBase<SocketCommandContext> {
        private readonly IGuildService _guildService;

        public AdminModule(IGuildService guildService) {
            _guildService = guildService;
        }

        [Name("Toggle Permission")]
        [Command("permission")]
        [Summary("Toggle a permission!")]
        public Task TogglePermission(IRole role, Permissions permission) {
            bool isAllowed = _guildService.ToggleRole(role, permission);
            StringBuilder message = new StringBuilder();
            message.Append($"Sucessfully toggled the role.");
            if (isAllowed) {
                message.Append($" {role.Name} are now granted the permission {permission}");
            } else {
                message.Append($" {permission} revoked for {role.Name}");
            }

            EmbedBuilder builder = new EmbedBuilder();
            builder.Title = $"Success";
            builder.Description = message.ToString();
            return ReplyAsync(embed:builder.Build());
        }

    }
}