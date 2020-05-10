using System.Threading.Tasks;
using Discord.Commands;
using DiscordBotFanatic.Helpers;

namespace DiscordBotFanatic.Modules {
    public class InfoModule : ModuleBase<SocketCommandContext> {
        [Command("info")]
        [Summary("Geeft informatie over de server")]
        public Task Info() {
            return ReplyAsync(
                $"Hello {Context.User.Username}, I am a bot called {Context.Client.CurrentUser.Username} written in Discord.Net 2.1.\nI'm currently in the server {Context.Guild.Name} - {Context.Guild.Description} and we're talking in the channel {Context.Channel.Name}.");
        }

        [Command("format")]
        public Task Format(int number) {
            return ReplyAsync($"{number.FormatNumber()}");
        }

        [Command("level")]
        public Task GetLevel(int number) {
            return ReplyAsync($"{number.ToLevel()}");
        }
    }
}