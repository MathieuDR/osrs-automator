using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
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

        [Name("Joined at list")]
        [Command("joinedList")]
        [Summary("print a CSV list of user & Joined date")]
        public async Task PrintJoinedList() {

            List<string> messages = new List<string>();
            StringBuilder csv = new StringBuilder();

            csv.AppendLine("User;nickname;JoinedAt;isbot");
            foreach (SocketGuildUser guildUser in Context.Guild.Users) {
                var line =
                    $"{guildUser.Username}#{guildUser.Discriminator};{guildUser.Nickname};{guildUser.JoinedAt};{guildUser.IsBot}";

                if (csv.Length + line.Length >= 2000) {
                    messages.Add(csv.ToString());
                    csv = new StringBuilder();
                }

                csv.AppendLine(line);
            }

            messages.Add(csv.ToString());

            EmbedBuilder builder = new EmbedBuilder();
            builder.Title = $"Success";
            await ReplyAsync(embed:builder.Build());

            foreach (string message in messages) {
                await ReplyAsync(message);
            }
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