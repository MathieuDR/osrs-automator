using System.Threading.Tasks;
using AutoMapper;
using Discord;
using Discord.Commands;
using DiscordBotFanatic.Helpers;
using DiscordBotFanatic.Models.Configuration;
using DiscordBotFanatic.Services.interfaces;

namespace DiscordBotFanatic.Modules {
    public class GroupModule : BaseWaitMessageEmbeddedResponseModule {
        private readonly IGroupService _groupService;

        public GroupModule(Mapper mapper, ILogService logger, MessageConfiguration messageConfiguration,
            IGroupService groupService) : base(mapper, logger,
            messageConfiguration) {
            _groupService = groupService;
        }

        [Name("Look at the top gains")]
        [Command("top", RunMode = RunMode.Async)]
        [Summary("Look at the top gains of competition or clan")]
        [RequireContext(ContextType.Guild)]
        public async Task TopGains() {
            // Get Leaderboard
            var leaderboardDecorator = await _groupService.GetGroupLeaderboard(GetGuildUser());

            // Print leaderboard out
            var embedBuilder = new EmbedBuilder()
                .AddWiseOldMan(leaderboardDecorator)
                .AddFooterFromMessageAuthor(Context);

            embedBuilder = Mapper.Map(leaderboardDecorator.Item, embedBuilder);

            // send
            await ModifyWaitMessageAsync(embedBuilder.Build());
        }
    }
}