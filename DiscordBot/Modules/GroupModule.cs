using System.Threading.Tasks;
using AutoMapper;
using Discord.Commands;
using DiscordBotFanatic.Models.Configuration;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Modules {
    public class GroupModule : BaseWaitMessageEmbeddedResponseModule {
        public GroupModule(Mapper mapper, ILogService logger, MessageConfiguration messageConfiguration) : base(mapper, logger,
            messageConfiguration) { }

        [Name("Look at the top gains")]
        [Command("top", RunMode = RunMode.Async)]
        [Summary("Look at the top gains of competition or clan")]
        [RequireContext(ContextType.Guild)]
        public async Task TopGains() { }
    }
}