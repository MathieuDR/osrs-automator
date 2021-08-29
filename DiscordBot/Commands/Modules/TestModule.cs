using System.Threading.Tasks;
using Discord.Commands;

namespace DiscordBot.Commands.Modules {
    public class TestModule : ModuleBase<SocketCommandContext> {
        [Name("Ping me")]
        [Command("ping")]
        [Summary("Ping me")]
        [Alias("Hi","Hello","Sup")]
        [RequireContext(ContextType.Guild)]
        public async Task AddOsrsName([Remainder] string info = null) {
            var response = string.IsNullOrEmpty(info) ? "Hey there, stranger" : $"Scram with your extra text: '{info}'";
            
            await Context.Channel.SendMessageAsync(response);
        }
    }
}
